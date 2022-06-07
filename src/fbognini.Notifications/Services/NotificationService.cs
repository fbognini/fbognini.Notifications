using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Linq;
using System.Collections.Generic;
using fbognini.Notifications.Models;
using fbognini.Notifications.Queries;
using System;

namespace fbognini.Notifications.Services
{
    public abstract class NotificationService: INotificationService
    {
        protected string Id { get; set; }
        protected string ConnectionString { get; set; }
        protected string Schema { get; set; }

        protected NotificationService(string id, string connectionString, string schema)
        {
            ConnectionString = connectionString;
            Schema = schema;

            if (!string.IsNullOrWhiteSpace(id))
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
