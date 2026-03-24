using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Octokit;
using Utils.General;
using GitCredentials = LibGit2Sharp.UsernamePasswordCredentials;
using GitHubCredentials = Octokit.Credentials;
using GitHubRepository = Octokit.Repository;
using GitRepository = LibGit2Sharp.Repository;
using GitSignature = LibGit2Sharp.Signature;

public class GitHub
{
    private string _Token { get; }
    private ILogger _Logger { get; }
    private GitHubClient _Client { get; }
    public string Name { get; set; }
    public string Email { get; set; }
    private GitSignature _Signature => new GitSignature(Name, Email, DateTimeOffset.Now);

    public GitHub(string token, string name, string email, ILogger logger)
    {
        _Logger = logger;

        _Token = token;
        _Client = new GitHubClient(new ProductHeaderValue("GitHubSyncService"))
        {
            Credentials = new GitHubCredentials(_Token)
        };
        Name = name;
        Email = email;

        _Logger.LogDebug("GitHub client initialized for user '{Name}' <{Email}>", name, email);
    }

    #region GIT (LibGit2Sharp)

    private static void ValidateRepoUrl(string repoUrl)
    {
        if (string.IsNullOrWhiteSpace(repoUrl))
            throw new ArgumentException("Repository URL cannot be null or empty.", nameof(repoUrl));

        if (Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri))
        {
            if ((uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) &&
                uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) &&
                uri.AbsolutePath.Count(c => c == '/') >= 2)
            {
                return;
            }
        }

        var sshPattern = @"^git@github\.com:[\w\.-]+\/[\w\.-]+(\.git)?$";
        if (System.Text.RegularExpressions.Regex.IsMatch(repoUrl, sshPattern))
        {
            return;
        }

        throw new ArgumentException($"Invalid GitHub repository URL format: '{repoUrl}'", nameof(repoUrl));
    }

    public void CloneIfMissing(string repoUrl, string localPath)
    {
        ValidateRepoUrl(repoUrl);

        if (!Directory.Exists(localPath))
        {
            _Logger.LogInformation("Local path '{LocalPath}' not found — cloning from '{RepoUrl}'", localPath, repoUrl);

            Helper.CreateFolderIfDoesNotExist(localPath);

            var co = new CloneOptions();
            co.FetchOptions.CredentialsProvider =
                (_url, _user, _cred) => new UsernamePasswordCredentials { Username = "token", Password = _Token };

            GitRepository.Clone(repoUrl, localPath, co);

            _Logger.LogInformation("Clone completed to '{LocalPath}'", localPath);
            return;
        }

        _Logger.LogDebug("Local path '{LocalPath}' exists — validating repository", localPath);

        if (!GitRepository.IsValid(localPath))
        {
            throw new InvalidOperationException(
                $"Directory '{localPath}' exists but is not a valid Git repository.");
        }

        using var repo = new GitRepository(localPath);
        Remote? origin = repo.Network.Remotes["origin"];

        if (origin == null)
        {
            throw new InvalidOperationException(
                $"Repository at '{localPath}' does not contain an 'origin' remote.");
        }

        _Logger.LogDebug("Repository valid — origin remote: '{OriginUrl}'", origin.Url);
    }

    public GitRepository OpenRepository(string localPath)
    {
        _Logger.LogDebug("Opening repository at '{LocalPath}'", localPath);
        return new GitRepository(localPath);
    }

    public void Pull(GitRepository repo)
    {
        _Logger.LogInformation("Pulling latest changes — branch: '{Branch}'", repo.Head.FriendlyName);

        var fetchOptions = new FetchOptions
        {
            CredentialsProvider = (_, _, _) => new GitCredentials
            {
                Username = "token",
                Password = _Token
            }
        };

        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(r => r.Specification).ToList();

        _Logger.LogDebug("Fetching from '{RemoteName}' with refspecs: {RefSpecs}", remote.Name, string.Join(", ", refSpecs));

        Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, "pre-pull fetch");

        _Logger.LogDebug("Fetch complete — resolving tracked branch");

        var branch = repo.Head;
        var trackedBranch = branch.TrackedBranch;

        if (trackedBranch == null)
        {
            _Logger.LogWarning("No tracked branch found for '{Branch}' — falling back to origin/main or origin/master", branch.FriendlyName);

            trackedBranch = repo.Branches["origin/main"]
                ?? repo.Branches["origin/master"]
                ?? throw new InvalidOperationException("No tracked remote branch found after fetch.");
        }

        _Logger.LogDebug("Merging '{TrackedBranch}' → '{LocalBranch}'", trackedBranch.FriendlyName, branch.FriendlyName);

        var result = repo.Merge(trackedBranch, _Signature, new MergeOptions
        {
            FastForwardStrategy = FastForwardStrategy.FastForwardOnly
        });

        _Logger.LogInformation("Merge result: {MergeStatus}", result.Status);
    }

    public bool HasChanges(GitRepository repo)
    {
        var isDirty = repo.RetrieveStatus().IsDirty;
        _Logger.LogDebug("Repository has changes: {HasChanges}", isDirty);
        return isDirty;
    }

    public void StageAll(GitRepository repo)
    {
        _Logger.LogDebug("Staging all changes");
        Commands.Stage(repo, "*");
        _Logger.LogDebug("Staging complete");
    }

    public void Commit(GitRepository repo, string message)
    {
        _Logger.LogInformation("Committing with message: '{CommitMessage}'", message);
        repo.Commit(message, _Signature, _Signature);
        _Logger.LogDebug("Commit created — SHA: {Sha}", repo.Head.Tip.Sha);
    }

    public void Push(GitRepository repo)
    {
        var refSpec = $"refs/heads/{repo.Head.FriendlyName}:refs/heads/{repo.Head.FriendlyName}";
        _Logger.LogInformation("Pushing '{Branch}' to origin", repo.Head.FriendlyName);

        repo.Network.Push(repo.Network.Remotes["origin"], refSpec, new PushOptions
        {
            CredentialsProvider = (_, _, _) => new GitCredentials
            {
                Username = "token",
                Password = _Token
            }
        });

        _Logger.LogInformation("Push complete — branch: '{Branch}'", repo.Head.FriendlyName);
    }

    #endregion

    #region GITHUB (Octokit)

    public async Task<User> GetCurrentUserAsync()
    {
        _Logger.LogDebug("Fetching current GitHub user");
        return await _Client.User.Current();
    }

    public async Task<GitHubRepository> CreateRepositoryAsync(string name, bool isPrivate)
    {
        _Logger.LogInformation("Creating repository '{RepoName}' (private: {IsPrivate})", name, isPrivate);
        return await _Client.Repository.Create(new NewRepository(name) { Private = isPrivate });
    }

    public async Task<IReadOnlyList<GitHubRepository>> GetRepositoriesAsync()
    {
        _Logger.LogDebug("Fetching all repositories for current user");
        return await _Client.Repository.GetAllForCurrent();
    }

    public async Task<Issue> CreateIssueAsync(string owner, string repo, string title, string body)
    {
        _Logger.LogInformation("Creating issue '{Title}' in {Owner}/{Repo}", title, owner, repo);
        return await _Client.Issue.Create(owner, repo, new NewIssue(title) { Body = body });
    }

    public async Task<PullRequest> CreatePullRequestAsync(string owner, string repo, string title, string head, string baseBranch, string body)
    {
        _Logger.LogInformation("Creating pull request '{Title}' — {Head} → {Base} in {Owner}/{Repo}", title, head, baseBranch, owner, repo);
        return await _Client.PullRequest.Create(owner, repo, new NewPullRequest(title, head, baseBranch) { Body = body });
    }

    public async Task<Release> CreateReleaseAsync(string owner, string repo, string tag, string name, string body)
    {
        _Logger.LogInformation("Creating release '{Tag}' in {Owner}/{Repo}", tag, owner, repo);
        return await _Client.Repository.Release.Create(owner, repo, new NewRelease(tag) { Name = name, Body = body });
    }

    #endregion

    public async Task SyncAsync(string repoUrl, string localPath, string commitMessage)
    {
        _Logger.LogInformation("Starting sync — repo: '{RepoUrl}', path: '{LocalPath}'", repoUrl, localPath);

        CloneIfMissing(repoUrl, localPath);

        using var repo = OpenRepository(localPath);

        var remote = repo.Network.Remotes["origin"];
        var remoteRefs = repo.Network.ListReferences(remote, (_, _, _) => new GitCredentials
        {
            Username = "token",
            Password = _Token
        }).ToList();

        _Logger.LogDebug("Remote refs found: {Count}", remoteRefs.Count);

        if (remoteRefs.Any())
            Pull(repo);
        else
            _Logger.LogWarning("Remote is empty — skipping pull");

        if (HasChanges(repo))
        {
            StageAll(repo);
            Commit(repo, commitMessage);
            Push(repo);
            _Logger.LogInformation("Sync complete — changes committed and pushed");
        }
        else
        {
            _Logger.LogInformation("Sync complete — no changes to commit");
        }
    }
}