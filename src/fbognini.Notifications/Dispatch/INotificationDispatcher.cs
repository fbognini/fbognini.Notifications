using fbognini.Notifications.Abstractions;

namespace fbognini.Notifications.Dispatch;

public interface INotificationDispatcher
{
    Task<DispatchReport> DispatchAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}

public sealed record DispatchOutcome(string Channel, string SinkName, NotificationResult Result, int Attempts);

/// <summary>
/// Fan-out is best effort with a per-channel outcome: all-or-nothing across heterogeneous channels
/// cannot be implemented honestly, since an email already accepted by the SMTP server cannot be recalled
/// because Telegram then refused.
/// </summary>
public sealed class DispatchReport
{
    public required IReadOnlyList<DispatchOutcome> Outcomes { get; init; }

    public bool AllSucceeded => Outcomes.Count > 0 && Outcomes.All(o => o.Result.Success);

    public bool AnySucceeded => Outcomes.Any(o => o.Result.Success);

    public IEnumerable<DispatchOutcome> Failures => Outcomes.Where(o => !o.Result.Success);
}

public sealed class DispatcherOptions
{
    public int MaxAttempts { get; set; } = 3;

    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
}
