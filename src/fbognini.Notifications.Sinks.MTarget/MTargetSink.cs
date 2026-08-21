using fbognini.Notifications.Abstractions;

namespace fbognini.Notifications.Sinks.MTarget;

internal sealed class MTargetSink(IMTargetSender sender, MTargetSinkOptions options) : INotificationSink
{
    public string Channel => MTargetChannel.Name;

    public string Name => options.Name;

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var numbers = request.Recipients
            .Where(r => string.Equals(r.Channel, MTargetChannel.Name, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.Address)
            .ToArray();

        if (numbers.Length == 0)
        {
            return NotificationResult.Skipped("No MTarget recipients in this request.");
        }

        try
        {
            var result = await sender
                .SendAsync(request.ConfigurationId, numbers, request.Body, cancellationToken)
                .ConfigureAwait(false);

            return result.Success
                ? NotificationResult.Sent()
                : NotificationResult.PermanentFailure(result.Error ?? "MTarget rejected the message.");
        }
        catch (HttpRequestException ex)
        {
            return NotificationResult.TransientFailure(ex.Message);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return NotificationResult.TransientFailure("The MTarget request timed out.");
        }
    }
}
