using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Sinks.Email
{
    internal class EmailService : BaseEmailService
    {
        public EmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService)
            : this(settingsProvider, templateService, emailQueueService, null)
        {
        }

        public EmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService, string? id)
            : base(settingsProvider, templateService, emailQueueService, id)
        {
        }
    }
}
