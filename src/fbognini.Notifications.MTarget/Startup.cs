using fbognini.Notifications.Interfaces;
using fbognini.Notifications.MTarget.Services;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fbognini.Notifications.MTarget
{
    public static class Startup
    {
        public static IServiceCollection AddMTargetService(this IServiceCollection services, Action<DatabaseSettings>  action)
        {
            DatabaseSettings settings = new DatabaseSettings();
            action.Invoke(settings);

            return services
                .AddSingleton(settings)
                .AddTransient<ISmsService, MTargetService>()
                ;
        }
    }
}
