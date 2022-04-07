using Dapper;
using fbognini.Notifications.Queries;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace fbognini.Notifications
{
    public static class ExtensionMethods
    {
        public static RawEmailService AddRawEmailService(this IServiceProvider provider, string id, string schema = "notification")
        {
            return new RawEmailService(id, schema);
        }

        public static EmailService AddEmailService(this IServiceProvider provider, string id, string connectionString, string schema = "notification")
        {
            return new EmailService(id, connectionString, schema);
        }
    }
}
