using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Models.Sms;
using fbognini.Notifications.MTarget.Sdk;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fbognini.Notifications.MTarget.Services
{
    public class MTargetSmsService : BaseSmsService
    {
        private readonly IMTargetService mTargetService;

        public MTargetSmsService(DatabaseSettings settings, IMTargetService mTargetService)
            : this(null, settings, mTargetService)
        {
        }


        public MTargetSmsService(string id, DatabaseSettings settings, IMTargetService mTargetService)
            : base(id, settings.ConnectionString, settings.Schema)
        {
            this.mTargetService = mTargetService;
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
