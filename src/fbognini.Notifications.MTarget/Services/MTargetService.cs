using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Models.Sms;
using MTarget;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fbognini.Notifications.MTarget.Services
{
    public class MTargetService : ISmsService
    {
        protected string Id { get; set; }
        protected string ConnectionString { get; set; }
        protected string Schema { get; set; }

        internal SmsConfig Settings { get; set; }

        internal MTargetService(string id, string connectionString, string schema)
        {
            ConnectionString = connectionString;
            Schema = schema;

            if (!string.IsNullOrWhiteSpace(id))
            {
                ChangeId(id);
            }
        }

        protected void LoadSmsSettings()
        {
            Settings = Utils.GetSmsSettings(Id, ConnectionString, Schema);
        }

        public void ChangeId(string id)
        {
            Id = id;
            LoadSmsSettings();
        }

        public async Task<SmsResult> SendSms(string message, string phoneNumber)
        {
            return await this.SendSmss(message, new List<string> { phoneNumber });
        }

        public async Task<SmsResult> SendSmss(string message, List<string> phoneNumbers)
        {
            var client = new WSApiSmsClient();
            var mapEntry = new mapEntry[] { new mapEntry { key = "sender", value = Settings.Sender }, new mapEntry { key = "serviceid", value = Settings.ServiceId } };
            var response = await client.sendSMSAsync(Settings.Username, Settings.Password, phoneNumbers.ToArray(), message, mapEntry);
            return new SmsResult(response.@return[0].resultcode != -1, response.@return[0].reason);
        }
    }
}
