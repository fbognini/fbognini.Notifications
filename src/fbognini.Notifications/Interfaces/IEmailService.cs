using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace fbognini.Notifications.Interfaces
{
    public interface IEmailService: INotificationService
    {
        void Send(string? to, string subject, string message, bool isHtml = false);
        Task SendAsync(string? to, string subject, string message, bool isHtml = false, CancellationToken cancellationToken = default);
        void Send(string? to, string? cc, string? bcc, string subject, string message, bool isHtml = false, List<string>? attachments = null);
        Task SendAsync(string? to, string? cc, string? bcc, string subject, string message, bool isHtml = false, List<string>? attachments = null, CancellationToken cancellationToken = default);
        void Schedule(List<Email> emails);
        EmailTemplate GetTemplate(string name);
        EmailTemplate GetTemplateById(string id);
    }
}
