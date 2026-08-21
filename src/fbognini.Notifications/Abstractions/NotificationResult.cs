namespace fbognini.Notifications.Abstractions;

/// <summary>
/// The transient/permanent split is what lets the dispatcher retry without guessing: a 429 or a dropped
/// socket is worth another attempt, a malformed address never is.
/// </summary>
public sealed class NotificationResult
{
    private NotificationResult(bool success, string? failureReason, bool isTransient, TimeSpan? retryAfter)
    {
        Success = success;
        FailureReason = failureReason;
        IsTransient = isTransient;
        RetryAfter = retryAfter;
    }

    public bool Success { get; }

    public string? FailureReason { get; }

    public bool IsTransient { get; }

    /// <summary>Honoured by the dispatcher when the channel dictates its own backoff, as Telegram does.</summary>
    public TimeSpan? RetryAfter { get; }

    public static NotificationResult Sent() => new(true, null, false, null);

    public static NotificationResult Skipped(string reason) => new(true, reason, false, null);

    public static NotificationResult TransientFailure(string reason, TimeSpan? retryAfter = null)
        => new(false, reason, true, retryAfter);

    public static NotificationResult PermanentFailure(string reason) => new(false, reason, false, null);
}
