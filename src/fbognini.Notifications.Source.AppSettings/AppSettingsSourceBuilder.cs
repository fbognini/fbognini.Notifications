using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using fbognini.Notifications.Source.AppSettings.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Source.AppSettings
{
    public class AppSettingsSourceBuilder : SourceBuilder
    {
        internal AppSettingsSourceBuilder(IServiceCollection services)
            : base(services)
        {
        }

        internal AppSettingsSourceBuilder AddSettingsProvider(Settings.AppSettings settings)
        {
            Services.AddScoped<ISettingsProvider, AppSettingsSettingsProvider>((provider) => new AppSettingsSettingsProvider(settings));

            return this;
        }

        internal AppSettingsSourceBuilder AddTemplateService(Settings.AppSettings settings)
        {
            Services.AddTransient<ITemplateService, AppSettingsTemplateService>((provider) => new AppSettingsTemplateService(settings));

            return this;
        }
        internal AppSettingsSourceBuilder AddEmailQueueService(Settings.AppSettings settings)
        {
            Services.AddTransient<IEmailQueueService, AppSettingsEmailQueueService>((provider) => new AppSettingsEmailQueueService(settings));

            return this;
        }
    }
}
