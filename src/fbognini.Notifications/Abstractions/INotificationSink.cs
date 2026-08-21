namespace fbognini.Notifications.Abstractions;

/// <summary>
/// Everything the dispatcher needs from a channel. Implementations must be stateless and thread-safe:
/// the per-profile configuration is resolved on each send from the configuration provider, never held
/// on the instance.
/// </summary>
public interface INotificationSink
{
    string Channel { get; }

    /// <summary>Distinguishes two sinks serving the same channel.</summary>
    string Name { get; }

    Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
