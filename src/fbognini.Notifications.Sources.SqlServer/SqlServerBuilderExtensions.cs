using fbognini.Notifications.Builder;

namespace fbognini.Notifications.Sources.SqlServer;

public static class SqlServerBuilderExtensions
{
    /// <summary>
    /// Registers the dynamic layer plus the template and queue capabilities. No schema is created here:
    /// the DDL lives in schema.sql and is applied by whoever owns the database, not as a side effect of
    /// reading a configuration.
    /// </summary>
    public static NotificationsBuilder FromSqlServer(
        this NotificationsBuilder builder,
        Action<SqlServerSourceOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SqlServerSourceOptions();
        configure(options);
        options.Validate();

        return builder
            .AddDynamicConfigurationSource(_ => new SqlServerConfigurationSource(options))
            .AddTemplateStore(_ => new SqlServerTemplateStore(options))
            .AddQueue(_ => new SqlServerNotificationQueue(options));
    }
}
