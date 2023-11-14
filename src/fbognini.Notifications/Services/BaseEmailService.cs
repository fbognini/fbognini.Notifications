using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Notifications.Services
{
    public abstract class BaseEmailService : NotificationService, IEmailService
    {
        private readonly ITemplateService templateService;
        private readonly IEmailQueueService emailQueueService;

        protected EmailConfig? Settings { get; set; }

        public BaseEmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService)
            : base(settingsProvider)
        {
            this.templateService = templateService;
            this.emailQueueService = emailQueueService;
        }

        public BaseEmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService, string id)
            : base(id, settingsProvider)
        {
            this.templateService = templateService;
            this.emailQueueService = emailQueueService;
        }

        protected override void LoadSettings()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException(nameof(Id));
            }

            Settings = settingsProvider.GetEmailSettings(Id);
        }

        public void Send(string? to, string subject, string message, bool isHtml = false) => Send(to, null, null, subject, message, isHtml, null);
        public Task SendAsync(string? to, string subject, string message, bool isHtml = false, CancellationToken cancellationToken = default) => SendAsync(to, null, null, subject, message, isHtml, null, cancellationToken);

        public abstract void Send(string? to, string? cc, string? bcc, string subject, string message, bool isHtml = false, List<string>? attachments = null);
        public abstract Task SendAsync(string? to, string? cc, string? bcc, string subject, string message, bool isHtml = false, List<string>? attachments = null, CancellationToken cancellationToken = default);

        public void Schedule(List<Email> emails)
        {
            emailQueueService.InsertQueueEmails(emails);
        }

        public EmailTemplate GetTemplate(string name)
        {
            return templateService.GetTemplate(name);
        }

        public EmailTemplate GetTemplateById(string id)
        {
            return templateService.GetTemplateById(id);
        }
    }
}
