using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using System;
using System.Collections.Generic;

namespace fbognini.Notifications.Source.AppSettings
{
    internal class AppSettingsEmailQueueService : IEmailQueueService
    {
        private readonly Settings.AppSettings settings;

        public AppSettingsEmailQueueService(Settings.AppSettings settings)
        {
            this.settings = settings;
        }

        public int InsertQueueEmails(List<Email> emails)
        {
            throw new NotImplementedException("Unable to insert email into queue when appsettings source is used");
        }
    }
}
