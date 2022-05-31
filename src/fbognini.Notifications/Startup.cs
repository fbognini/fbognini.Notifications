using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fbognini.Notifications
{
    public static class Startup
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services, Action<DatabaseSettings>  action)
        {
            DatabaseSettings settings = new DatabaseSettings();
            action.Invoke(settings);

            return services
                .AddSingleton(settings)
                .AddTransient<ITemplateService, TemplateService>()
                .AddTransient<IEmailService, EmailService>()
                ;
        }
    }
}
