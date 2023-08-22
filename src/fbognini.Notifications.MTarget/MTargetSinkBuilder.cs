using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using fbognini.Notifications.Sinks.MTarget.Sdk;
using fbognini.Notifications.Sinks.MTarget.Services;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Notifications.Sinks.MTarget
{
    public class MTargetSinkBuilder : SinkBuilder
    {
        internal MTargetSinkBuilder(IServiceCollection services)
            : base(services)
        {
        }

        internal MTargetSinkBuilder AddMTargetService()
        {
            Services
                .AddTransient<ISmsService, MTargetSmsService>();

            return this;
        }

        internal MTargetSinkBuilder AddMTargetService(string id)
        {
            Services.AddHttpClient<IMTargetService, MTargetService>((sp, cl) =>
            {

            });

            Services
                .AddTransient<ISmsService, MTargetSmsService>((provider) => new MTargetSmsService(id, provider.GetRequiredService<ISettingsProvider>(), provider.GetRequiredService<IMTargetService>()));
            return this;
        }
    }
}
