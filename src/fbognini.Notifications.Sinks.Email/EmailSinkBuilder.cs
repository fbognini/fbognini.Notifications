using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Notifications.Sinks.Email
{
    public class EmailSinkBuilder : SinkBuilder
    {
        internal EmailSinkBuilder(IServiceCollection services)
            : base(services)
        {
        }

        internal EmailSinkBuilder AddEmailService()
        {
            Services.AddTransient<IEmailService, EmailService>();

            return this;
        }

        internal EmailSinkBuilder AddEmailService(string id)
        {
            Services.AddTransient<IEmailService, EmailService>((provider) => new EmailService(provider.GetRequiredService<ISettingsProvider>(), provider.GetRequiredService<ITemplateService>(), provider.GetRequiredService<IEmailQueueService>(), id));

            return this;
        }
    }
}
