using fbognini.Notifications.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace fbognini.Notifications.Sinks.Email;

internal sealed class EmailSink(IEmailSender sender, EmailSinkOptions options) : INotificationSink
{
    public string Channel => EmailChannel.Name;

    public string Name => options.Name;

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var addresses = request.Recipients
            .Where(r => string.Equals(r.Channel, EmailChannel.Name, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.Address)
            .ToArray();

        if (addresses.Length == 0)
        {
            return NotificationResult.Skipped("No email recipients in this request.");
        }

        var message = new EmailMessage
        {
            ConfigurationId = request.ConfigurationId,
            To = string.Join(";", addresses),
            Subject = string.Empty,
            Body = request.Body,
        };

        try
        {
            await sender.SendAsync(message, cancellationToken).ConfigureAwait(false);
            return NotificationResult.Sent();
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            return NotificationResult.TransientFailure(ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return NotificationResult.PermanentFailure(ex.Message);
        }
    }

    // A refused mailbox or a malformed address will be refused again; anything that smells like the
    // connection or the server being momentarily unavailable is worth another attempt.
    private static bool IsTransient(Exception ex) => ex switch
    {
        SmtpCommandException smtp => smtp.StatusCode is
            SmtpStatusCode.ServiceNotAvailable or
            SmtpStatusCode.MailboxBusy or
            SmtpStatusCode.InsufficientStorage or
            SmtpStatusCode.TransactionFailed,
        SmtpProtocolException => true,
        SslHandshakeException => false,
        IOException => true,
        System.Net.Sockets.SocketException => true,
        TimeoutException => true,
        _ => false,
    };
}
