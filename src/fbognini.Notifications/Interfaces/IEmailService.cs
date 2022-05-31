using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System.Collections.Generic;

namespace fbognini.Notifications.Interfaces
{
    public interface IMultiEmailService: IEmailService
    {
        void LoadConnectionString(string connectionString, string schema);
    }

    public interface IEmailService
    {
        void Send(string to, string subject, string message, bool isHtml = false);
        void Send(string to, string cc, string bcc, string subject, string message, bool isHtml = false, List<string> attachments = null);
        void Schedule(List<Email> emails);

        void ChangeId(string id);
        EmailTemplate GetTemplate(string name);
    }
}
