using fbognini.Notifications.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Source.AppSettings.Settings
{
    public class AppSettings
    {
        public List<EmailConfig>? EmailConfigs { get; set; }
        public List<SmsConfig>? SmsConfigs { get; set; }
        public string? EmailTemplatesPath { get; set; }
        
        public class EmailConfig
        {
            public string Id { get; set; }
            public bool UseSsl { get; set; }
            public string SmtpHost { get; set; }
            public int SmtpPort { get; set; }
            public bool UseAuthentication { get; set; }
            public string? SmtpUsername { get; set; }
            public string? SmtpPassword { get; set; }
            public string FromEmail { get; set; }
        }

        public class SmsConfig
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Sender { get; set; }
            public string ServiceId { get; set; }
        }
    }
}
