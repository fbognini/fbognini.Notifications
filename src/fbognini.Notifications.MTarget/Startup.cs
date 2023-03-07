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
        public static NotificationsBuilder AddMTargetService(this NotificationsBuilder builder)
        {
            builder.Services
                .AddTransient<ISmsService, MTargetService>();

            return builder;
        }

        public static NotificationsBuilder AddMTargetService(this NotificationsBuilder builder, string id)
        {
            builder.Services
                .AddTransient<ISmsService, MTargetService>((provider) => new MTargetService(id, provider.GetRequiredService<DatabaseSettings>()));

            return builder;
        }

        public static NotificationsBuilder AddMTargetPrivateService(this NotificationsBuilder builder)
        {
            builder.Services
                .AddTransient<ISmsService, MTargetPrivateService>();

            return builder;
        }

        public static NotificationsBuilder AddMTargetPrivateService(this NotificationsBuilder builder, string id)
        {
            builder.Services
                .AddTransient<ISmsService, MTargetPrivateService>((provider) => new MTargetPrivateService(id, provider.GetRequiredService<DatabaseSettings>()));

            return builder;
        }
    }
}
