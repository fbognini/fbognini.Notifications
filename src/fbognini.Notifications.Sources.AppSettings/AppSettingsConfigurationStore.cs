using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using fbognini.Notifications.Configuration;
using Microsoft.Extensions.Configuration;

namespace fbognini.Notifications.Sources.AppSettings;

/// <summary>
/// Reads profiles laid out as {section}:{channel}:{id}. The store knows nothing about which channels
/// exist: whatever keys are present are offered to whichever sink asks for them.
/// </summary>
internal sealed class AppSettingsConfigurationStore(IConfiguration configuration, string sectionName)
    : IStaticConfigurationStore
{
    private readonly ConcurrentDictionary<(string Channel, string Id, Type Type), object?> bound = new();

    public bool TryGet<T>(string channel, string id, [MaybeNullWhen(false)] out T value) where T : class
    {
        var resolved = bound.GetOrAdd((channel, id, typeof(T)), key =>
        {
            var section = configuration.GetSection(sectionName).GetSection(key.Channel).GetSection(key.Id);

            return section.Exists() ? section.Get<T>() : null;
        });

        value = resolved as T;
        return value is not null;
    }

    public void Validate(string channel, Type identityType)
    {
        var section = configuration.GetSection(sectionName).GetSection(channel);

        foreach (var profile in section.GetChildren())
        {
            try
            {
                if (profile.Get(identityType) is null)
                {
                    throw new InvalidOperationException("the section is empty");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Notification profile '{sectionName}:{channel}:{profile.Key}' cannot be bound to {identityType.Name}: {ex.Message}",
                    ex);
            }
        }
    }
}
