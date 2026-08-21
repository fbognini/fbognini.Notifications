using fbognini.Notifications.Builder;
using fbognini.Notifications.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Notifications.Sinks.Telegram;

public static class TelegramBuilderExtensions
{
    public static NotificationsBuilder AddTelegram(
        this NotificationsBuilder builder,
        Action<TelegramSinkOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new TelegramSinkOptions();
        configure?.Invoke(options);

        builder.Services
            .AddHttpClient(HttpClientName(options.Name), client => client.Timeout = options.Timeout);

        builder.Services.AddKeyedSingleton<ITelegramSender>(options.Name, (sp, _) => new TelegramSender(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName(options.Name)),
            sp.GetRequiredService<INotificationConfigurationProvider>(),
            options));

        if (options.Name == TelegramChannel.DefaultSinkName)
        {
            builder.Services.AddSingleton<ITelegramSender>(sp =>
                sp.GetRequiredKeyedService<ITelegramSender>(TelegramChannel.DefaultSinkName));
        }

        return builder.AddSink(TelegramChannel.Name, options.Name, sp => new TelegramSink(
            sp.GetRequiredKeyedService<ITelegramSender>(options.Name),
            options), typeof(TelegramIdentity));
    }

    private static string HttpClientName(string sinkName) => $"fbognini.Notifications.Telegram.{sinkName}";
}
