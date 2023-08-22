using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System.Collections.Generic;

namespace fbognini.Notifications.Interfaces
{
    public interface IEmailQueueService
    {
        int InsertQueueEmails(List<Email> emails);
    }
}
