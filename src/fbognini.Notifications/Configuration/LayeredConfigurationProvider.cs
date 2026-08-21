using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace fbognini.Notifications.Configuration;

internal sealed class LayeredConfigurationProvider : INotificationConfigurationProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IStaticConfigurationStore? staticStore;
    private readonly INotificationConfigurationSource? dynamicSource;
    private readonly NotificationConfigurationOptions options;
    private readonly ILogger<LayeredConfigurationProvider> logger;
    private readonly DynamicConfigurationCache cache;

    public LayeredConfigurationProvider(
        NotificationConfigurationOptions options,
        ILogger<LayeredConfigurationProvider> logger,
        TimeProvider timeProvider,
        IStaticConfigurationStore? staticStore = null,
        INotificationConfigurationSource? dynamicSource = null)
    {
        this.options = options;
        this.logger = logger;
        this.staticStore = staticStore;
        this.dynamicSource = dynamicSource;

        cache = new DynamicConfigurationCache(timeProvider, options.DynamicCacheCapacity);
    }

    public async ValueTask<T> GetAsync<T>(string channel, string id, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (staticStore is not null && staticStore.TryGet<T>(channel, id, out var stat))
        {
            return stat;
        }

        if (dynamicSource is null)
        {
            throw new NotificationConfigurationNotFoundException(channel, id);
        }

        var key = (channel, id);

        if (cache.TryGetFresh(key, options.DynamicCacheTtl, out var cached))
        {
            return Deserialize<T>(cached, channel, id);
        }

        string? json;
        try
        {
            json = await dynamicSource.ReadAsync(channel, id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (options.ServeStaleOnSourceFailure && cache.TryGetStale(key, out var stale))
            {
                logger.LogWarning(
                    ex,
                    "Configuration source failed for channel {Channel} id {Id}; serving the last known value.",
                    channel,
                    id);

                return Deserialize<T>(stale, channel, id);
            }

            throw;
        }

        if (json is null)
        {
            throw new NotificationConfigurationNotFoundException(channel, id);
        }

        cache.Set(key, json);
        return Deserialize<T>(json, channel, id);
    }

    public void Invalidate(string channel, string id) => cache.Remove((channel, id));

    internal int CachedEntryCount => cache.Count;


    private static T Deserialize<T>(string json, string channel, string id) where T : class
    {
        T? value;
        try
        {
            value = JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Configuration for channel '{channel}' with id '{id}' could not be read as {typeof(T).Name}.", ex);
        }

        return value ?? throw new NotificationConfigurationNotFoundException(channel, id);
    }
}
