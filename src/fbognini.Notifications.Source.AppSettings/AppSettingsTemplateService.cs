using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace fbognini.Notifications.Source.AppSettings
{
    internal class AppSettingsTemplateService : ITemplateService
    {
        private readonly Settings.AppSettings settings;

        public AppSettingsTemplateService(Settings.AppSettings settings)
        {
            this.settings = settings;
        }

        public EmailTemplate GetTemplate(string name)
        {
            var templates = GetTemplates();

            return templates.First(x => x.Name == name);
        }

        public EmailTemplate GetTemplateById(string id)
        {
            var templates = GetTemplates();

            return templates.First(x => x.Id == id);
        }

        private List<EmailTemplate> GetTemplates()
        {
            if (string.IsNullOrEmpty(settings.EmailTemplatesPath))
            {
                throw new Exception("Email template file path not specified");
            }

            var str = File.ReadAllText(settings.EmailTemplatesPath);
            return JsonSerializer.Deserialize<List<EmailTemplate>>(str);
        }
    }
}
