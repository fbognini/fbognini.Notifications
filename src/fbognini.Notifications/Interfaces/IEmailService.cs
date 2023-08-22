using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System.Collections.Generic;

namespace fbognini.Notifications.Interfaces
{
    public interface IEmailService: INotificationService
    {
        void Send(string to, string subject, string message, bool isHtml = false);
        void Send(string to, string cc, string bcc, string subject, string message, bool isHtml = false, List<string> attachments = null);
        void Schedule(List<Email> emails);
        EmailTemplate GetTemplate(string name);
        EmailTemplate GetTemplateById(string id);
    }
}
