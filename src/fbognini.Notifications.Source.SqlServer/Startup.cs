using fbognini.Notifications.Services;
using fbognini.Notifications.Source.SqlServer.Settings;
using System;

namespace fbognini.Notifications.Source.SqlServer
{
    public static class Startup
    {
        public static SqlServerSourceBuilder FromSqlServer(this SinkBuilder services, Action<DatabaseSettings> action)
        {
            DatabaseSettings settings = new DatabaseSettings();
            action.Invoke(settings);

            var builder = new SqlServerSourceBuilder(services.Services)
                .AddSettingsProvider(settings)
                .AddTemplateService(settings)
                .AddEmailQueueService(settings);

            return builder;
        }
    }
}
