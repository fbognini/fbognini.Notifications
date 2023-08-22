using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fbognini.Notifications.Services
{
    public abstract class BaseEmailService : NotificationService, IEmailService
    {
        private readonly ITemplateService templateService;
        private readonly IEmailQueueService emailQueueService;

        private EmailConfig Settings { get; set; }

        public BaseEmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService)
            : this(settingsProvider, templateService, emailQueueService, null)
        {
        }

        public BaseEmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService, string id)
            : base(id, settingsProvider)
        {
            this.templateService = templateService;
            this.emailQueueService = emailQueueService;
        }

        protected override void LoadSettings()
        {
            Settings = settingsProvider.GetEmailSettings(Id);
        }

        public void Send(string to, string subject, string message, bool isHtml = false)
        {
            // create message
            Send(to, null, null, subject, message, isHtml, null);
        }

        public void Send(string to, string cc, string bcc, string subject, string message, bool isHtml = false, List<string> attachments = null)
        {
            if (Settings == null)
                throw new NotImplementedException("Settings are not configured");

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(Settings.FromEmail));

            if (!string.IsNullOrWhiteSpace(to))
            {
                foreach (var item in to.Split(new char[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    email.To.Add(MailboxAddress.Parse(item.Trim()));
                }
            }

            if (!string.IsNullOrWhiteSpace(cc))
            {
                foreach (var item in cc.Split(new char[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    email.Cc.Add(MailboxAddress.Parse(item.Trim()));
                }
            }

            if (!string.IsNullOrWhiteSpace(bcc))
            {
                foreach (var item in bcc.Split(new char[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    email.Bcc.Add(MailboxAddress.Parse(item.Trim()));
                }
            }

            if (email.To.Count() == 0 && email.Cc.Count() == 0 && email.Bcc.Count() == 0)
                return;

            email.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml)
                builder.HtmlBody = message;
            else
                builder.TextBody = message;

            if (attachments != null)
                attachments.ForEach(attachment => builder.Attachments.Add(attachment));

            email.Body = builder.ToMessageBody();

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(Settings.SmtpHost, Settings.SmtpPort, Settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            if (Settings.UseAuthentication)
                smtp.Authenticate(Settings.SmtpUsername, Settings.SmtpPassword);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public void Schedule(List<Models.Email> emails)
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
