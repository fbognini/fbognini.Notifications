using fbognini.Notifications.Configuration;
using Microsoft.Extensions.Hosting;

namespace fbognini.Notifications.Builder;

public sealed class NotificationCompositionException(string message) : Exception(message);

internal static class CompositionValidator
{
    public static void Validate(NotificationsRegistry registry, IStaticConfigurationStore? staticStore = null)
    {
        if (registry.Sinks.Count == 0)
        {
            throw new NotificationCompositionException(
                "No notification sink is registered. Add at least one, for example AddEmail() or AddTelegram().");
        }

        if (!registry.HasStaticStore && !registry.HasDynamicSource)
        {
            var channels = string.Join(", ", registry.Sinks.Select(s => s.Channel).Distinct());

            throw new NotificationCompositionException(
                $"Sinks are registered for {channels} but no configuration source is available. " +
                "Add FromAppSettings(...), FromSqlServer(...), or both.");
        }

        if (staticStore is null)
        {
            return;
        }

        foreach (var sink in registry.Sinks.Where(s => s.IdentityType is not null))
        {
            staticStore.Validate(sink.Channel, sink.IdentityType!);
        }
    }
}

internal sealed class CompositionValidationService(
    NotificationsRegistry registry,
    IStaticConfigurationStore? staticStore = null) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        CompositionValidator.Validate(registry, staticStore);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
