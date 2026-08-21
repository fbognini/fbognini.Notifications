using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using fbognini.Notifications.Abstractions;
using fbognini.Notifications.Configuration;

namespace fbognini.Notifications.Tests;

internal sealed class FakeDynamicSource : INotificationConfigurationSource
{
    private readonly ConcurrentDictionary<(string, string), string> payloads = new();

    public int Reads;

    public Exception? ThrowOnRead { get; set; }

    public void Set(string channel, string id, object value)
        => payloads[(channel, id)] = JsonSerializer.Serialize(value);

    public ValueTask<string?> ReadAsync(string channel, string id, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Reads);

        if (ThrowOnRead is not null)
        {
            throw ThrowOnRead;
        }

        return ValueTask.FromResult(payloads.TryGetValue((channel, id), out var json) ? json : null);
    }
}

internal sealed class FakeStaticStore : IStaticConfigurationStore
{
    private readonly Dictionary<(string, string), object> values = [];

    public void Set(string channel, string id, object value) => values[(channel, id)] = value;

    public bool TryGet<T>(string channel, string id, [MaybeNullWhen(false)] out T value) where T : class
    {
        if (values.TryGetValue((channel, id), out var stored) && stored is T typed)
        {
            value = typed;
            return true;
        }

        value = null;
        return false;
    }

    public void Validate(string channel, Type identityType)
    {
    }
}

internal sealed class RecordingSink(string channel, string name) : INotificationSink
{
    private readonly List<NotificationRequest> received = [];

    public IReadOnlyList<NotificationRequest> Received => received;

    public int Attempts { get; private set; }

    public Queue<NotificationResult> Results { get; } = new();

    public string Channel => channel;

    public string Name => name;

    public Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        Attempts++;

        lock (received)
        {
            received.Add(request);
        }

        return Task.FromResult(Results.Count > 0 ? Results.Dequeue() : NotificationResult.Sent());
    }
}
