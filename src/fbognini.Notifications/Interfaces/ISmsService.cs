using fbognini.Notifications.Models;
using fbognini.Notifications.Models.Sms;
using fbognini.Notifications.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fbognini.Notifications.Interfaces
{
    public interface ISmsService
    {
        Task<SmsResult> SendSms(string message, string phoneNumber);
        Task<SmsResult> SendSmss(string message, List<string> phoneNumbers);
    }
}
