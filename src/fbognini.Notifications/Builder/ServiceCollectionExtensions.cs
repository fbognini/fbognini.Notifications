using fbognini.Notifications.Builder;
using fbognini.Notifications.Configuration;
using fbognini.Notifications.Dispatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace fbognini.Notifications;

public static class ServiceCollectionExtensions
{
    public static NotificationsBuilder AddNotifications(
        this IServiceCollection services,
        Action<NotificationConfigurationOptions>? configureConfiguration = null,
        Action<DispatcherOptions>? configureDispatcher = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var configurationOptions = new NotificationConfigurationOptions();
        configureConfiguration?.Invoke(configurationOptions);

        var dispatcherOptions = new DispatcherOptions();
        configureDispatcher?.Invoke(dispatcherOptions);

        var registry = new NotificationsRegistry();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(registry);
        services.AddSingleton(configurationOptions);
        services.AddSingleton(dispatcherOptions);

        services.AddSingleton<INotificationConfigurationProvider>(sp => new LayeredConfigurationProvider(
            sp.GetRequiredService<NotificationConfigurationOptions>(),
            sp.GetRequiredService<ILogger<LayeredConfigurationProvider>>(),
            sp.GetRequiredService<TimeProvider>(),
            sp.GetService<IStaticConfigurationStore>(),
            sp.GetService<INotificationConfigurationSource>()));

        services.AddSingleton<INotificationDispatcher, NotificationDispatcher>();
        services.AddSingleton<IHostedService>(sp => new CompositionValidationService(
            sp.GetRequiredService<NotificationsRegistry>(),
            sp.GetService<IStaticConfigurationStore>()));

        return new NotificationsBuilder(services, registry);
    }
}
