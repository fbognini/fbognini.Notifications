using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace fbognini.Notifications.Services
{
    public class NotificationsBuilder
    {
        public IServiceCollection Services { get; set; }

        internal NotificationsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        internal NotificationsBuilder AddDatabaseSettings(DatabaseSettings settings)
        {
            Services.AddSingleton(settings);

            return this;
        }
    }
}
