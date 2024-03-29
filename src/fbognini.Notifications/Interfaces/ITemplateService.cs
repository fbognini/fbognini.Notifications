﻿using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using System.Collections.Generic;

namespace fbognini.Notifications.Interfaces
{
    public interface ITemplateService
    {
        EmailTemplate GetTemplate(string name);
        EmailTemplate GetTemplateById(string id);
    }
}
