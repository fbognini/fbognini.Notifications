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
using System.Threading.Tasks;
using fbognini.Notifications.Models.Sms;

namespace fbognini.Notifications.Services
{
    public abstract class BaseSmsService : NotificationService, ISmsService
    {
        protected SmsConfig Settings { get; set; }

        protected BaseSmsService(string id, string connectionString, string schema)
            : base(id, connectionString, schema)
        {            
        }

        protected override void LoadSettings()
        {
            Settings = Utils.GetSmsSettings(Id, ConnectionString, Schema);
        }

        public abstract Task<SmsResult> SendSms(string message, string phoneNumber);

        public abstract Task<SmsResult> SendSmss(string message, List<string> phoneNumbers);
    }

}
