using fbognini.Notifications.Builder;
using fbognini.Notifications.Configuration;
using fbognini.Notifications.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Notifications.Sinks.Email;

public static class EmailBuilderExtensions
{
    /// <summary>
    /// Registers the SMTP sink. Per-profile credentials come from the configuration source at send time;
    /// only what is fixed for the whole process is configured here.
    /// </summary>
    public static NotificationsBuilder AddEmail(
        this NotificationsBuilder builder,
        Action<EmailSinkOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new EmailSinkOptions();
        configure?.Invoke(options);

        builder.Services.AddKeyedSingleton<IEmailSender>(options.Name, (sp, _) => new EmailSender(
            sp.GetRequiredService<INotificationConfigurationProvider>(),
            options,
            sp.GetService<INotificationQueue>()));

        if (options.Name == EmailChannel.DefaultSinkName)
        {
            builder.Services.AddSingleton<IEmailSender>(sp =>
                sp.GetRequiredKeyedService<IEmailSender>(EmailChannel.DefaultSinkName));
        }

        return builder.AddSink(EmailChannel.Name, options.Name, sp => new EmailSink(
            sp.GetRequiredKeyedService<IEmailSender>(options.Name),
            options), typeof(EmailIdentity));
    }
}
