using System;

namespace fbognini.Notifications.Models
{
    public class EmailTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
