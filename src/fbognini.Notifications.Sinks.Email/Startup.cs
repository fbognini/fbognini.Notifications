using fbognini.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Sinks.Email
{
    public static class Startup
    {
        public static EmailSinkBuilder AddEmailService(this SinkBuilder services)
        {
            var builder = new EmailSinkBuilder(services.Services)
                .AddEmailService();

            return builder;
        }

        public static EmailSinkBuilder AddEmailService(this SinkBuilder services, string id)
        {
            var builder = new EmailSinkBuilder(services.Services)
                .AddEmailService(id);

            return builder;
        }
    }
}
