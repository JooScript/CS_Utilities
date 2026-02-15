using LibGit2Sharp;
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
    private GitHubClient _Client { get; }
    public string Name { get; set; }
    public string Email { get; set; }
    private GitSignature _Signature => new GitSignature(Name, Email, DateTimeOffset.Now);

    public GitHub(string token, string name, string email)
    {
        _Token = token;
        _Client = new GitHubClient(new ProductHeaderValue("GitHubSyncService"))
        {
            Credentials = new GitHubCredentials(_Token)
        };
        Name = name;
        Email = email;
    }

    #region GIT (LibGit2Sharp)

    private static void ValidateRepoUrl(string repoUrl)
    {
        if (string.IsNullOrWhiteSpace(repoUrl))
            throw new ArgumentException("Repository URL cannot be null or empty.", nameof(repoUrl));

        // HTTPS validation
        if (Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri))
        {
            if ((uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) &&
                uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) &&
                uri.AbsolutePath.Count(c => c == '/') >= 2)
            {
                return;
            }
        }

        // SSH validation (git@github.com:owner/repo.git)
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
            Helper.CreateFolderIfDoesNotExist(localPath);

            var co = new CloneOptions();

            co.FetchOptions.CredentialsProvider =
                (_url, _user, _cred)
                => new UsernamePasswordCredentials
                { Username = "token", Password = _Token };

            GitRepository.Clone(repoUrl, localPath, co);
            return;
        }

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


    }



    public GitRepository OpenRepository(string localPath)
    {
        return new GitRepository(localPath);
    }

    public void Pull(GitRepository repo)
    {
        Commands.Pull(repo, _Signature, new PullOptions
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = (_, _, _) => new GitCredentials
                {
                    Username = "token",
                    Password = _Token
                }
            }
        });
    }

    public bool HasChanges(GitRepository repo)
    {
        return repo.RetrieveStatus().IsDirty;
    }

    public void StageAll(GitRepository repo)
    {
        Commands.Stage(repo, "*");
    }

    public void Commit(GitRepository repo, string message)
    {
        repo.Commit(message, _Signature, _Signature);
    }

    public void Push(GitRepository repo)
    {
        repo.Network.Push(repo.Head, new PushOptions
        {
            CredentialsProvider = (_, _, _) => new GitCredentials
            {
                Username = "token",
                Password = _Token
            }
        });
    }

    #endregion

    #region GITHUB (Octokit)

    public async Task<User> GetCurrentUserAsync()
    {
        return await _Client.User.Current();
    }

    public async Task<GitHubRepository> CreateRepositoryAsync(string name, bool isPrivate)
    {
        return await _Client.Repository.Create(new NewRepository(name) { Private = isPrivate });
    }

    public async Task<IReadOnlyList<GitHubRepository>> GetRepositoriesAsync()
    {
        return await _Client.Repository.GetAllForCurrent();
    }

    public async Task<Issue> CreateIssueAsync(string owner, string repo, string title, string body)
    {
        return await _Client.Issue.Create(owner, repo, new NewIssue(title) { Body = body });
    }

    public async Task<PullRequest> CreatePullRequestAsync(string owner, string repo, string title, string head, string baseBranch, string body)
    {
        return await _Client.PullRequest.Create(owner, repo, new NewPullRequest(title, head, baseBranch) { Body = body });
    }

    public async Task<Release> CreateReleaseAsync(string owner, string repo, string tag, string name, string body)
    {
        return await _Client.Repository.Release.Create(owner, repo, new NewRelease(tag) { Name = name, Body = body });
    }

    #endregion

    public async Task SyncAsync(string repoUrl, string localPath, string commitMessage)
    {
        CloneIfMissing(repoUrl, localPath);

        using var repo = OpenRepository(localPath);

        Pull(repo);

        if (HasChanges(repo))
        {
            StageAll(repo);
            Commit(repo, commitMessage);
            Push(repo);
        }

    }

}