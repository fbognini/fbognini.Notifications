using fbognini.Notifications.Abstractions;
using fbognini.Notifications.Configuration;
using fbognini.Notifications.Queue;
using fbognini.Notifications.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Notifications.Builder;

/// <summary>
/// Accumulates the composition. Sink and source packages extend this type; the core never learns which
/// channels exist.
/// </summary>
public sealed class NotificationsBuilder(IServiceCollection services, NotificationsRegistry registry)
{
    public IServiceCollection Services { get; } = services;

    public NotificationsRegistry Registry { get; } = registry;

    /// <summary>
    /// Registers a sink under its channel and name. Registering two sinks on the same channel is
    /// supported and both receive; the name is what keyed resolution addresses.
    /// </summary>
    public NotificationsBuilder AddSink(
        string channel,
        string name,
        Func<IServiceProvider, INotificationSink> factory,
        Type? identityType = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Registry.AddSink(channel, name, identityType);

        Services.AddSingleton(factory);
        Services.AddKeyedSingleton<INotificationSink>(name, (sp, _) =>
            sp.GetServices<INotificationSink>().Single(s => s.Name == name));

        return this;
    }

    public NotificationsBuilder AddStaticConfigurationStore(Func<IServiceProvider, IStaticConfigurationStore> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Registry.MarkStaticStore();
        Services.AddSingleton(factory);

        return this;
    }

    public NotificationsBuilder AddDynamicConfigurationSource(
        Func<IServiceProvider, INotificationConfigurationSource> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Registry.MarkDynamicSource();
        Services.AddSingleton(factory);

        return this;
    }

    public NotificationsBuilder AddTemplateStore(Func<IServiceProvider, ITemplateStore> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Registry.MarkTemplateStore();
        Services.AddSingleton(factory);

        return this;
    }

    public NotificationsBuilder AddQueue(Func<IServiceProvider, INotificationQueue> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Registry.MarkQueue();
        Services.AddSingleton(factory);

        return this;
    }

    /// <summary>
    /// Validates the accumulated composition immediately. Calling it is optional: the same checks run at
    /// host start, so a misconfigured application fails there rather than on its first send.
    /// </summary>
    public NotificationsBuilder Build()
    {
        CompositionValidator.Validate(Registry);
        return this;
    }
}
