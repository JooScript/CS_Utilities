using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Utils.Format;

namespace Utils.IMAP;

public sealed class Imap(ILogger logger, string host, int port, string user, string pass, List<string> Mailboxes)
{
    /// <summary>
    /// Downloads every message from each configured mailbox via IMAP/SSL
    /// and saves it as an RFC-822 .eml file.  Already-saved files are skipped.
    ///
    /// MailKit docs: https://github.com/jstedfast/MailKit
    /// </summary>
    public async Task<int> BackupAllAsync(DirectoryInfo backupRoot, CancellationToken cancellationToken = default)
    {
        int totalNew = 0;

        using var client = new ImapClient();

        logger.LogInformation("Connecting to {Host}:{Port}…", host, port);

        await client.ConnectAsync(host, port,
            SecureSocketOptions.SslOnConnect, cancellationToken);

        logger.LogInformation("Authenticating as {User}…", user);

        await client.AuthenticateAsync(user, pass, cancellationToken);

        foreach (var mailbox in Mailboxes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            totalNew += await BackupMailboxAsync(client, mailbox, backupRoot, cancellationToken);
        }

        await client.DisconnectAsync(quit: true, cancellationToken);
        return totalNew;
    }

    private async Task<int> BackupMailboxAsync(
        ImapClient client,
        string mailboxName,
        DirectoryInfo root,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Mailbox}] Opening…", mailboxName);

        IMailFolder folder;

        try
        {
            folder = await client.GetFolderAsync(mailboxName, cancellationToken);
        }
        catch (FolderNotFoundException)
        {
            logger.LogWarning("[{Mailbox}] Folder not found — skipping.", mailboxName);
            return 0;
        }

        await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        var uids = await folder.SearchAsync(SearchQuery.All, cancellationToken);

        logger.LogInformation($"[{mailboxName}] {uids.Count} total messages.");

        var dest = new DirectoryInfo(Path.Combine(root.FullName, FormatHelper.SanitizeName(mailboxName)));

        dest.Create();

        int saved = 0;

        foreach (var uid in uids)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filename = Path.Combine(dest.FullName, $"{uid.Id:D8}.eml");

            if (File.Exists(filename))
            {
                logger.LogDebug(
                    "[{Mailbox}] Skipping existing message UID {Uid}",
                    mailboxName,
                    uid.Id
                );
                continue;
            }

            logger.LogInformation(
                "[{Mailbox}] Saving message UID {Uid} ({Current}/{Total})",
                mailboxName,
                uid.Id,
                saved + 1,
                uids.Count
            );

            var message = await folder.GetMessageAsync(uid, cancellationToken);

            // Prepend subject as a comment for human readability
            var subject = message.Subject ?? "(no subject)";
            await using var stream = File.Create(filename);
            await message.WriteToAsync(stream, cancellationToken);

            saved++;
        }

        logger.LogInformation("[{Mailbox}] {Saved} new emails saved.", mailboxName, saved);
        await folder.CloseAsync(expunge: false, cancellationToken);
        return saved;
    }

}
