using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System;
using System.Linq;

namespace fbognini.Notifications.Source.AppSettings
{
    internal class AppSettingsSettingsProvider : ISettingsProvider
    {
        private readonly Settings.AppSettings settings;

        public AppSettingsSettingsProvider(Settings.AppSettings settings)
        {
            this.settings = settings;
        }

        public EmailConfig GetEmailSettings(string id)
        {
            var emailConfig = settings.EmailConfigs?.FirstOrDefault(x => x.Id == id);

            if (emailConfig is null)
            {
                throw new Exception($"Email configuration not found with id {id}");
            }

            return new EmailConfig()
            {
                FromEmail = emailConfig.FromEmail,
                SmtpHost = emailConfig.SmtpHost,
                SmtpPassword = emailConfig.SmtpPassword,
                SmtpPort = emailConfig.SmtpPort,
                SmtpUsername = emailConfig.SmtpUsername,
                UseAuthentication = emailConfig.UseAuthentication,
                UseSsl = emailConfig.UseSsl,
            };
        }

        public SmsConfig GetSmsSettings(string id)
        {
            var smsConfig = settings.SmsConfigs?.FirstOrDefault(x => x.Id == id);

            if (smsConfig is null)
            {
                throw new Exception($"Sms configuration not found with id {id}");
            }

            return new SmsConfig()
            {
                Password = smsConfig.Password,
                Sender = smsConfig.Sender,
                ServiceId = smsConfig.ServiceId,
                Username = smsConfig.Username
            };
        }
    }
}
