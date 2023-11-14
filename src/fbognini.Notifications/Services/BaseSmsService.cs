using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Models.Sms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fbognini.Notifications.Services
{
    public abstract class BaseSmsService : NotificationService, ISmsService
    {
        protected SmsConfig Settings { get; set; }

        protected BaseSmsService(string id, ISettingsProvider settingsProvider)
            : base(id, settingsProvider)
        {            
        }

        protected override void LoadSettings()
        {
            Settings = settingsProvider.GetSmsSettings(Id);
        }

        public abstract Task<SmsResult> SendSms(string message, string phoneNumber);

        public abstract Task<SmsResult> SendSmss(string message, List<string> phoneNumbers);
    }
}
