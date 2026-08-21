using fbognini.Notifications.Abstractions;
using Microsoft.Extensions.Logging;

namespace fbognini.Notifications.Dispatch;

internal sealed class NotificationDispatcher(
    IEnumerable<INotificationSink> sinks,
    DispatcherOptions options,
    ILogger<NotificationDispatcher> logger,
    TimeProvider timeProvider) : INotificationDispatcher
{
    private readonly IReadOnlyList<INotificationSink> sinks = sinks.ToArray();

    public async Task<DispatchReport> DispatchAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var channels = request.Recipients.Select(r => r.Channel).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var work = new List<Task<DispatchOutcome>>();

        foreach (var channel in channels)
        {
            var targets = sinks.Where(s => string.Equals(s.Channel, channel, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (targets.Length == 0)
            {
                work.Add(Task.FromResult(new DispatchOutcome(
                    channel,
                    string.Empty,
                    NotificationResult.PermanentFailure($"No sink is registered for channel '{channel}'."),
                    0)));

                continue;
            }

            var scoped = request.ForChannel(channel);

            foreach (var sink in targets)
            {
                work.Add(SendWithRetryAsync(sink, scoped, cancellationToken));
            }
        }

        var outcomes = await Task.WhenAll(work).ConfigureAwait(false);

        return new DispatchReport { Outcomes = outcomes };
    }

    private async Task<DispatchOutcome> SendWithRetryAsync(
        INotificationSink sink,
        NotificationRequest request,
        CancellationToken cancellationToken)
    {
        NotificationResult result;
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                result = await sink.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Sink {Sink} threw while sending on channel {Channel}.", sink.Name, sink.Channel);
                result = NotificationResult.PermanentFailure(ex.Message);
            }

            if (result.Success || !result.IsTransient || attempt >= options.MaxAttempts)
            {
                return new DispatchOutcome(sink.Channel, sink.Name, result, attempt);
            }

            var delay = result.RetryAfter ?? TimeSpan.FromTicks(options.BaseDelay.Ticks * (1L << (attempt - 1)));

            logger.LogWarning(
                "Sink {Sink} reported a transient failure ({Reason}); retrying in {Delay}.",
                sink.Name,
                result.FailureReason,
                delay);

            await Task.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);
        }
    }
}
