using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fbognini.Notifications
{
    public static class Startup
    {
        public static NotificationsBuilder AddNotifications(this IServiceCollection services)
        {
            var builder = new NotificationsBuilder(services);

            return builder;
        }
    }
}
