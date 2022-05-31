using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System.Collections.Generic;

namespace fbognini.Notifications.Interfaces
{
    internal interface ITemplateService
    {
        void LoadConfiguration(string connectionString, string schema);
        EmailTemplate GetTemplate(string name);
    }
}
