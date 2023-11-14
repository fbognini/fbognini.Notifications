using fbognini.Notifications.Interfaces;
using System;

namespace fbognini.Notifications.Services
{
    public abstract class NotificationService : INotificationService
    {
        protected readonly ISettingsProvider settingsProvider;
        protected string? Id { get; set; }

        protected NotificationService(ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
        }

        protected NotificationService(string id, ISettingsProvider settingsProvider)
            : this(settingsProvider)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(nameof(id));
            }

            ChangeId(id);
        }

        protected abstract void LoadSettings();

        public void ChangeId(string id)
        {
            Id = id;
            LoadSettings();
        }
    }

}
