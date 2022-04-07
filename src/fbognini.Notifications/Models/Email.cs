using System;

namespace fbognini.Notifications.Models
{
    public class Email
    {
        public int Id { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public string Attachments { get; set; }
        public DateTime InsertionDate { get; set; }
        public bool Processing { get; set; }
        public int ErrorRetry { get; set; }
        public string ErrorMessage { get; set; }
    }
}
