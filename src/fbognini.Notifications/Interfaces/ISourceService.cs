using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;

namespace fbognini.Notifications.Interfaces
{
    public interface ISettingsProvider
    {
        public EmailConfig GetEmailSettings(string id);
        public SmsConfig GetSmsSettings(string id);
    }
}
