namespace fbognini.Notifications.Builder;

/// <summary>
/// What the builder accumulated. Kept separate from the service collection so the composition can be
/// validated as a whole at host start, instead of failing one resolution at a time on the first send.
/// </summary>
public sealed class NotificationsRegistry
{
    private readonly List<SinkRegistration> sinks = [];

    public IReadOnlyList<SinkRegistration> Sinks => sinks;

    public bool HasStaticStore { get; private set; }

    public bool HasDynamicSource { get; private set; }

    public void AddSink(string channel, string name, Type? identityType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (sinks.Any(s => s.Channel == channel && s.Name == name))
        {
            throw new InvalidOperationException(
                $"A sink named '{name}' is already registered for channel '{channel}'. Give the second one a distinct name.");
        }

        sinks.Add(new SinkRegistration(channel, name, identityType));
    }

    public void MarkStaticStore() => HasStaticStore = true;

    public void MarkDynamicSource() => HasDynamicSource = true;
}

public sealed record SinkRegistration(string Channel, string Name, Type? IdentityType);
