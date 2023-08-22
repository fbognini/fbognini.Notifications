using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Models.Sms;
using fbognini.Notifications.Sinks.MTarget.Sdk;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fbognini.Notifications.Sinks.MTarget.Services
{
    public class MTargetSmsService : BaseSmsService
    {
        private readonly IMTargetService mTargetService;

        public MTargetSmsService(ISettingsProvider settingsProvider, IMTargetService mTargetService)
            : this(null, settingsProvider, mTargetService)
        {
        }


        public MTargetSmsService(string id, ISettingsProvider settingsProvider, IMTargetService mTargetService)
            : base(id, settingsProvider)
        {
            this.mTargetService = mTargetService;
            this.mTargetService.ChangeSettings(new MTargetSettings()
            {
                Username = Settings.Username,
                Password = Settings.Password,
                Sender = Settings.Sender
            });
        }

        public override async Task<SmsResult> SendSms(string message, string phoneNumber)
        {
            return await SendSmss(message, new List<string> { phoneNumber });
        }

        public override async Task<SmsResult> SendSmss(string message, List<string> phoneNumbers)
        {
            if (Settings == null)
                throw new NotImplementedException("Settings are not configured");


            var response = await mTargetService.SendMessages(phoneNumbers, message);
            var result = response.Results.First();
            return new SmsResult(result.Code != "-1", result.Reason);
        }
    }
}
