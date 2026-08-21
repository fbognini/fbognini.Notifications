using fbognini.Notifications.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace fbognini.Notifications.Sinks.Email;

internal sealed class EmailSender(
    INotificationConfigurationProvider configuration,
    EmailSinkOptions options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var identity = await configuration
            .GetAsync<EmailIdentity>(EmailChannel.Name, message.ConfigurationId, cancellationToken)
            .ConfigureAwait(false);

        var mime = BuildMessage(message, identity);
        if (mime is null)
        {
            return;
        }

        using var smtp = new SmtpClient { Timeout = (int)options.Timeout.TotalMilliseconds };

        await smtp.ConnectAsync(
            identity.SmtpHost,
            identity.SmtpPort,
            identity.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            cancellationToken).ConfigureAwait(false);

        if (identity.UseAuthentication)
        {
            if (identity.SmtpUsername is null || identity.SmtpPassword is null)
            {
                throw new InvalidOperationException(
                    $"Email profile '{message.ConfigurationId}' sets UseAuthentication but is missing SmtpUsername or SmtpPassword.");
            }

            await smtp.AuthenticateAsync(identity.SmtpUsername, identity.SmtpPassword, cancellationToken)
                .ConfigureAwait(false);
        }

        await smtp.SendAsync(mime, cancellationToken).ConfigureAwait(false);
        await smtp.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
    }

    internal static MimeMessage? BuildMessage(EmailMessage message, EmailIdentity identity)
    {
        var mime = new MimeMessage();

        mime.From.Add(string.IsNullOrWhiteSpace(identity.FromName)
            ? MailboxAddress.Parse(identity.FromEmail)
            : new MailboxAddress(identity.FromName, identity.FromEmail));

        if (!string.IsNullOrWhiteSpace(identity.ReplyToEmail))
        {
            mime.ReplyTo.Add(MailboxAddress.Parse(identity.ReplyToEmail));
        }

        AddAddresses(mime.To, message.To);
        AddAddresses(mime.Cc, message.Cc);
        AddAddresses(mime.Bcc, message.Bcc);

        if (mime.To.Count == 0 && mime.Cc.Count == 0 && mime.Bcc.Count == 0)
        {
            return null;
        }

        mime.Subject = message.Subject;

        var body = new BodyBuilder();
        if (message.IsHtml)
        {
            body.HtmlBody = message.Body;
        }
        else
        {
            body.TextBody = message.Body;
        }

        if (message.Attachments is not null)
        {
            foreach (var attachment in message.Attachments)
            {
                body.Attachments.Add(attachment);
            }
        }

        mime.Body = body.ToMessageBody();

        return mime;
    }

    private static void AddAddresses(InternetAddressList list, string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        foreach (var item in raw.Split([';', ','], StringSplitOptions.RemoveEmptyEntries))
        {
            list.Add(MailboxAddress.Parse(item.Trim()));
        }
    }
}
