using System.Diagnostics.CodeAnalysis;

namespace fbognini.Notifications.Configuration;

/// <summary>
/// The single entry point for per-profile configuration. Callers never know which layer answered.
/// </summary>
public interface INotificationConfigurationProvider
{
    ValueTask<T> GetAsync<T>(string channel, string id, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Drops the cached dynamic entry so the next read hits the source. This is the hook for the case
    /// where the application changes a tenant's configuration through logic that does not go through
    /// this library. Static entries are immutable for the life of the process and ignore this call.
    /// </summary>
    void Invalidate(string channel, string id);
}

/// <summary>
/// The static layer: profiles known at startup, materialised once, never reloaded. Optional — an
/// application may run entirely on dynamic configuration.
/// </summary>
public interface IStaticConfigurationStore
{
    bool TryGet<T>(string channel, string id, [MaybeNullWhen(false)] out T value) where T : class;

    /// <summary>
    /// Binds every profile of a channel to the identity type its sink declared, throwing on the first
    /// one that does not fit. Called once at host start.
    /// </summary>
    void Validate(string channel, Type identityType);
}

/// <summary>
/// The dynamic layer: profiles read from a backing store at runtime. Returns the raw serialised payload
/// so a source never needs to know which types the sinks use — that is what makes a new sink a new
/// package instead of a change to this one. Optional.
/// </summary>
public interface INotificationConfigurationSource
{
    ValueTask<string?> ReadAsync(string channel, string id, CancellationToken cancellationToken = default);
}

public sealed class NotificationConfigurationNotFoundException : Exception
{
    public NotificationConfigurationNotFoundException(string channel, string id)
        : base($"No configuration found for channel '{channel}' with id '{id}'.")
    {
        Channel = channel;
        Id = id;
    }

    public string Channel { get; }

    public string Id { get; }
}
