namespace fbognini.Notifications.Queue;

/// <summary>
/// Optional source capability for deferred delivery. The payload is the serialised channel-specific
/// message, so the queue stays agnostic of the channels a deployment happens to use.
/// </summary>
public interface INotificationQueue
{
    Task<int> EnqueueAsync(
        IReadOnlyList<QueuedNotification> notifications,
        CancellationToken cancellationToken = default);
}

public sealed class QueuedNotification
{
    public required string Channel { get; init; }

    public required string ConfigurationId { get; init; }

    public required string Address { get; init; }

    public required string Payload { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
