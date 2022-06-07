using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fbognini.Notifications
{
    public static class Startup
    {
        public static NotificationsBuilder AddNotifications(this IServiceCollection services, Action<DatabaseSettings> action)
        {
            DatabaseSettings settings = new DatabaseSettings();
            action.Invoke(settings);

            var builder = new NotificationsBuilder(services)
                .AddDatabaseSettings(settings);

            return builder;
        }

        public static NotificationsBuilder AddEmailService(this NotificationsBuilder builder)
        {
            builder.Services
                .AddTransient<ITemplateService, TemplateService>()
                .AddTransient<IEmailService, EmailService>();

            return builder;
        }
    }
}
