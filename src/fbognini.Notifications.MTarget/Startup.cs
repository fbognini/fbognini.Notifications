using fbognini.Notifications.Interfaces;
using fbognini.Notifications.MTarget.Sdk;
using fbognini.Notifications.MTarget.Services;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fbognini.Notifications.MTarget
{
    public static class Startup
    {
        public static NotificationsBuilder AddMTargetService(this NotificationsBuilder builder)
        {
            builder.Services
                .AddTransient<ISmsService, MTargetSmsService>();

            return builder;
        }

        public static NotificationsBuilder AddMTargetService(this NotificationsBuilder builder, string id)
        {
            builder.Services.AddHttpClient<IMTargetService, MTargetService>((sp, cl) =>
            {

            });

            builder.Services
                .AddTransient<ISmsService, MTargetSmsService>((provider) => new MTargetSmsService(id, provider.GetRequiredService<DatabaseSettings>(), provider.GetRequiredService<IMTargetService>()));

            return builder;
        }
    }
}
