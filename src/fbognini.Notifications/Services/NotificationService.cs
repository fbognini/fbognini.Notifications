using fbognini.Notifications.Interfaces;

namespace fbognini.Notifications.Services
{
    public abstract class NotificationService : INotificationService
    {
        protected readonly ISettingsProvider settingsProvider;
        protected string Id { get; set; }

        protected NotificationService(string id, ISettingsProvider settingsProvider)
        {
            this.Id = id;
            this.settingsProvider = settingsProvider;

            if (!string.IsNullOrEmpty(Id))
            {
                ChangeId(id);
            }
        }

        protected abstract void LoadSettings();

        public void ChangeId(string id)
        {
            Id = id;
            LoadSettings();
        }
    }

}
