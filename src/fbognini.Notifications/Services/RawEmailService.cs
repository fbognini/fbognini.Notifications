using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Linq;
using System.Collections.Generic;
using fbognini.Notifications.Models;
using fbognini.Notifications.Queries;
using System;

namespace fbognini.Notifications.Services
{
    internal class MultiEmailService : BaseEmailService, IMultiEmailService
    {
        internal MultiEmailService(ITemplateService templateService) : base(templateService)
        {
        }

        public void LoadConnectionString(string connectionString, string schema)
        {
            ConnectionString = connectionString;
            Schema = schema;

            LoadQueueQueries();
        }
    }
}
