namespace fbognini.Notifications.Settings
{
    public class EmailConfig
    {
        public bool UseSsl { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool UseAuthentication { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string? ReplyToEmail { get; set; }
    }
}
