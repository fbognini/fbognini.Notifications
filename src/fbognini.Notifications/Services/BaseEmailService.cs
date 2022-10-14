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
    public abstract class BaseEmailService: NotificationService, IEmailService
    {
        private readonly ITemplateService templateService;

        private EmailConfig Settings { get; set; }
        private ManageQueueEmailsQueries QueueQueries { get; set; }

        internal BaseEmailService(ITemplateService templateService)
            : this(templateService, null, null, null)
        {
        }

        internal BaseEmailService(ITemplateService templateService, string id, string connectionString, string schema)
            : base(id, connectionString, schema)
        {
            this.templateService = templateService;

            LoadQueueQueries();
        }

        protected void LoadQueueQueries()
        {
            QueueQueries = new ManageQueueEmailsQueries(ConnectionString, Schema);
        }

        protected override void LoadSettings()
        {
            Settings = Utils.GetEmailSettings(Id, ConnectionString, Schema);
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

        public void Schedule(List<Email> emails)
        {
            if (QueueQueries == null)
                throw new NotImplementedException("Scheduling is not configured");

            QueueQueries.InsertQueueEmails(emails);
        }

        public EmailTemplate GetTemplate(string name)
        {
            templateService.LoadConfiguration(ConnectionString, Schema);
            return templateService.GetTemplate(name);
        }

        public EmailTemplate GetTemplateById(string id)
        {
            templateService.LoadConfiguration(ConnectionString, Schema);
            return templateService.GetTemplateById(id);
        }


    }

}
