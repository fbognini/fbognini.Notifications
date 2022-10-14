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
    internal class EmailService : BaseEmailService
    {
        public EmailService(ITemplateService templateService, DatabaseSettings settings)
            : this(templateService, null, settings)
        {

        }

        public EmailService(ITemplateService templateService, string id, DatabaseSettings settings)
            : base(templateService, id, settings.ConnectionString, settings.Schema)
        {

        }
    }
}
