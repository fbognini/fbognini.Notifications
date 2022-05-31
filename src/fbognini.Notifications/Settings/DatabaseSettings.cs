namespace fbognini.Notifications.Settings
{
    public class DatabaseSettings
    {
        public string Id { get; set; }
        public string ConnectionString { get; set; }
        public string Schema { get; set; } = "notification";
    }
}
