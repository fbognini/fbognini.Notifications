using fbognini.Notifications.Builder;
using Microsoft.Extensions.Configuration;

namespace fbognini.Notifications.Sources.AppSettings;

public static class AppSettingsBuilderExtensions
{
    public const string DefaultSectionName = "Notifications";

    /// <summary>
    /// Registers the static layer. These profiles are read once and never reload: appsettings is treated
    /// as fixed for the life of the process, and anything that has to change at runtime belongs to a
    /// dynamic source instead.
    /// </summary>
    public static NotificationsBuilder FromAppSettings(
        this NotificationsBuilder builder,
        IConfiguration configuration,
        string sectionName = DefaultSectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        return builder.AddStaticConfigurationStore(_ =>
            new AppSettingsConfigurationStore(configuration, sectionName));
    }
}
