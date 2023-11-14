using fbognini.Notifications.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace fbognini.Notifications.Source.AppSettings
{
    public static class Startup
    {
        public static AppSettingsSourceBuilder FromAppSettings(this SinkBuilder services, IConfiguration configuration, string sectionName = "fbognini.Notifications")
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            var settings = configuration.GetSection(sectionName).Get<Settings.AppSettings>()!;

            var builder = new AppSettingsSourceBuilder(services.Services)
                .AddSettingsProvider(settings)
                .AddTemplateService(settings)
                .AddEmailQueueService(settings);

            return builder;
        }

        public static AppSettingsSourceBuilder FromAppSettings(this SinkBuilder services, Action<Settings.AppSettings> action)
        {
            Settings.AppSettings settings = new();
            
            action.Invoke(settings);

            var builder = new AppSettingsSourceBuilder(services.Services)
                .AddSettingsProvider(settings)
                .AddTemplateService(settings)
                .AddEmailQueueService(settings);

            return builder;
        }
    }
}
