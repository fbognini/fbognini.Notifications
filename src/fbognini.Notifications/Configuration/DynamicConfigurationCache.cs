using System.Collections.Concurrent;

namespace fbognini.Notifications.Configuration;

/// <summary>
/// Bounded, evicting cache for the dynamic layer. The key is a tuple rather than a concatenated string
/// so two profiles can never collide on one entry, which in a multi-tenant setup would mean sending with
/// another tenant's credentials.
/// </summary>
internal sealed class DynamicConfigurationCache(TimeProvider timeProvider, int capacity)
{
    private readonly ConcurrentDictionary<(string Channel, string Id), Entry> entries = new();

    private sealed record Entry(string Json, long LoadedAtTicks);

    public bool TryGetFresh((string Channel, string Id) key, TimeSpan ttl, out string json)
    {
        if (entries.TryGetValue(key, out var entry) &&
            timeProvider.GetUtcNow().UtcTicks - entry.LoadedAtTicks < ttl.Ticks)
        {
            json = entry.Json;
            return true;
        }

        json = string.Empty;
        return false;
    }

    public bool TryGetStale((string Channel, string Id) key, out string json)
    {
        if (entries.TryGetValue(key, out var entry))
        {
            json = entry.Json;
            return true;
        }

        json = string.Empty;
        return false;
    }

    public void Set((string Channel, string Id) key, string json)
    {
        entries[key] = new Entry(json, timeProvider.GetUtcNow().UtcTicks);

        if (entries.Count > capacity)
        {
            Evict();
        }
    }

    public void Remove((string Channel, string Id) key) => entries.TryRemove(key, out _);

    public int Count => entries.Count;

    private void Evict()
    {
        var target = Math.Max(1, capacity / 10);

        foreach (var key in entries.OrderBy(x => x.Value.LoadedAtTicks).Take(target).Select(x => x.Key).ToArray())
        {
            entries.TryRemove(key, out _);
        }
    }
}
