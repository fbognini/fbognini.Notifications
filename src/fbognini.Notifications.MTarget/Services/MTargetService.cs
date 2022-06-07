using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Models.Sms;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using MTarget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fbognini.Notifications.MTarget.Services
{
    public class MTargetService : BaseSmsService
    {
        public MTargetService(DatabaseSettings settings)
            : this(null, settings)
        {
        }


        public MTargetService(string id, DatabaseSettings settings)
            : base(id, settings.ConnectionString, settings.Schema)
        {
        }

        public override async Task<SmsResult> SendSms(string message, string phoneNumber)
        {
            return await SendSmss(message, new List<string> { phoneNumber });
        }

        public override async Task<SmsResult> SendSmss(string message, List<string> phoneNumbers)
        {
            if (Settings == null)
                throw new NotImplementedException("Settings are not configured");

            var client = new WSApiSmsClient();
            var mapEntry = new mapEntry[] { new mapEntry { key = "sender", value = Settings.Sender }, new mapEntry { key = "serviceid", value = Settings.ServiceId } };
            var response = await client.sendSMSAsync(Settings.Username, Settings.Password, phoneNumbers.ToArray(), message, mapEntry);
            return new SmsResult(response.@return[0].resultcode != -1, response.@return[0].reason);
        }
    }
}
