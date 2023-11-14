using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using fbognini.Notifications.Source.SqlServer.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Source.SqlServer
{
    public class SqlServerSourceBuilder : SourceBuilder
    {
        internal SqlServerSourceBuilder(IServiceCollection services)
            : base(services)
        {
        }

        internal SqlServerSourceBuilder AddSettingsProvider(DatabaseSettings settings)
        {
            Services.AddSingleton<ISettingsProvider, SqlServerSettingsProvider>((provider) => new SqlServerSettingsProvider(settings));

            return this;
        }

        internal SqlServerSourceBuilder AddTemplateService(DatabaseSettings settings)
        {
            Services.AddTransient<ITemplateService, SqlServerTemplateService>((provider) => new SqlServerTemplateService(settings));

            return this;
        }
        internal SqlServerSourceBuilder AddEmailQueueService(DatabaseSettings settings)
        {
            Services.AddTransient<IEmailQueueService, SqlServerEmailQueueService>((provider) => new SqlServerEmailQueueService(settings));

            return this;
        }
    }
}
