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
    public abstract class BaseEmailService
    {
        private EmailSettings Settings { get; set; }
        private ManageQueueEmailsQueries QueueQueries { get; set; }

        public BaseEmailService()
        {
        }

        protected void LoadQueueQueries(string connectionString, string schema)
        {
            QueueQueries = new ManageQueueEmailsQueries(connectionString, schema);
        }

        protected void LoadEmailSettings(string id, string connectionString, string schema)
        {
            Settings = Utils.GetEmailSettings(id, connectionString, schema);
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
    }

    public class RawEmailService : BaseEmailService, IRawEmailService
    {
        private readonly string id;
        private readonly string schema;

        public RawEmailService(string id, string schema)
        {
            this.id = id;
            this.schema = schema;
        }

        public void LoadConnectionString(string connectionString)
        {
            LoadEmailSettings(id, connectionString, schema);
            LoadQueueQueries(connectionString, schema);
        }
    }

    public class EmailService : BaseEmailService, IEmailService
    {
        public EmailService(string id, string connectionString, string schema)
        {
            LoadEmailSettings(id, connectionString, schema);
            LoadQueueQueries(connectionString, schema);
        }
    }
}
