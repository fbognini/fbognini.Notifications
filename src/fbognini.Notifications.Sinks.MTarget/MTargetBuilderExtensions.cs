using fbognini.Notifications.Builder;
using fbognini.Notifications.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Notifications.Sinks.MTarget;

public static class MTargetBuilderExtensions
{
    public static NotificationsBuilder AddMTarget(
        this NotificationsBuilder builder,
        Action<MTargetSinkOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new MTargetSinkOptions();
        configure?.Invoke(options);

        builder.Services.AddHttpClient(HttpClientName(options.Name), client => client.Timeout = options.Timeout);

        builder.Services.AddKeyedSingleton<IMTargetSender>(options.Name, (sp, _) => new MTargetSender(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName(options.Name)),
            sp.GetRequiredService<INotificationConfigurationProvider>(),
            options));

        if (options.Name == MTargetChannel.DefaultSinkName)
        {
            builder.Services.AddSingleton<IMTargetSender>(sp =>
                sp.GetRequiredKeyedService<IMTargetSender>(MTargetChannel.DefaultSinkName));
        }

        return builder.AddSink(MTargetChannel.Name, options.Name, sp => new MTargetSink(
            sp.GetRequiredKeyedService<IMTargetSender>(options.Name),
            options), typeof(MTargetIdentity));
    }

    private static string HttpClientName(string sinkName) => $"fbognini.Notifications.MTarget.{sinkName}";
}
