using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Notifications.Sinks.Email
{
    internal class EmailService : BaseEmailService
    {
        public EmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService)
            : base(settingsProvider, templateService, emailQueueService)
        {
        }

        public EmailService(ISettingsProvider settingsProvider, ITemplateService templateService, IEmailQueueService emailQueueService, string id)
            : base(settingsProvider, templateService, emailQueueService, id)
        {
        }

        public override void Send(string? to, string? cc, string? bcc, string subject, string message, bool isHtml = false, List<string>? attachments = null)
        {
            var email = GetMimeMessage(to, cc, bcc, subject, message, isHtml, attachments);
            if (email is null)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(Settings);

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(Settings.SmtpHost, Settings.SmtpPort, Settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            if (Settings.UseAuthentication)
            {
                smtp.Authenticate(Settings.SmtpUsername, Settings.SmtpPassword);
            }
            smtp.Send(email);
            smtp.Disconnect(true);
        }



        public override async Task SendAsync(string? to, string? cc, string? bcc, string subject, string message, bool isHtml = false, List<string>? attachments = null, CancellationToken cancellationToken = default)
        {
            var email = GetMimeMessage(to, cc, bcc, subject, message, isHtml, attachments);
            if (email is null)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(Settings);

            // send email
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(Settings.SmtpHost, Settings.SmtpPort, Settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);
            if (Settings.UseAuthentication)
            {
                await smtp.AuthenticateAsync(Settings.SmtpUsername, Settings.SmtpPassword, cancellationToken);
            }
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }

        private MimeMessage? GetMimeMessage(string? to, string? cc, string? bcc, string subject, string message, bool isHtml = false, List<string>? attachments = null)
        {
            if (Settings == null)
            {
                throw new NotImplementedException("Settings are not configured");
            }

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(Settings.FromEmail));
            if (!string.IsNullOrWhiteSpace(Settings.ReplyToEmail))
            {
                email.ReplyTo.Add(MailboxAddress.Parse(Settings.ReplyToEmail));
            }

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

            if (email.To.Count == 0 && email.Cc.Count == 0 && email.Bcc.Count == 0)
            {
                return null;
            }

            email.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml)
            {
                builder.HtmlBody = message;
            }
            else
            {
                builder.TextBody = message;
            }

            attachments?.ForEach(attachment => builder.Attachments.Add(attachment));

            email.Body = builder.ToMessageBody();

            return email;
        }
    }
}
