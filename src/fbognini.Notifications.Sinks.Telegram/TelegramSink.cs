using fbognini.Notifications.Abstractions;

namespace fbognini.Notifications.Sinks.Telegram;

internal sealed class TelegramSink(ITelegramSender sender, TelegramSinkOptions options) : INotificationSink
{
    public string Channel => TelegramChannel.Name;

    public string Name => options.Name;

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var chats = request.Recipients
            .Where(r => string.Equals(r.Channel, TelegramChannel.Name, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.Address)
            .ToArray();

        if (chats.Length == 0)
        {
            return NotificationResult.Skipped("No Telegram recipients in this request.");
        }

        var failures = new List<string>();
        TimeSpan? retryAfter = null;

        foreach (var chat in chats)
        {
            TelegramSendResult result;
            try
            {
                result = await sender.SendAsync(
                    new TelegramMessage
                    {
                        ConfigurationId = request.ConfigurationId,
                        ChatId = chat,
                        Text = request.Body,
                    },
                    cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                return NotificationResult.TransientFailure(ex.Message);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return NotificationResult.TransientFailure("The Telegram request timed out.");
            }

            if (result.Success)
            {
                continue;
            }

            failures.Add($"{chat}: {result.Error}");

            if (result.RetryAfter is { } wait && (retryAfter is null || wait > retryAfter))
            {
                retryAfter = wait;
            }
        }

        if (failures.Count == 0)
        {
            return NotificationResult.Sent();
        }

        var reason = string.Join("; ", failures);

        return retryAfter is null
            ? NotificationResult.PermanentFailure(reason)
            : NotificationResult.TransientFailure(reason, retryAfter);
    }
}
