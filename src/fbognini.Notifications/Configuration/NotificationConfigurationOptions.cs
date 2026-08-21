namespace fbognini.Notifications.Configuration;

public sealed class NotificationConfigurationOptions
{
    public TimeSpan DynamicCacheTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Bounds the dynamic cache. Unlike the named-options cache behind IOptionsMonitor this one evicts,
    /// which is what makes it safe for tenant ids created at runtime.
    /// </summary>
    public int DynamicCacheCapacity { get; set; } = 512;

    /// <summary>
    /// When the dynamic source throws, serve the last known value even if its TTL has lapsed. A tenant
    /// that stops receiving notifications because the database hiccuped is the worse outcome.
    /// </summary>
    public bool ServeStaleOnSourceFailure { get; set; } = true;
}
