using fbognini.Notifications.Abstractions;
using fbognini.Notifications.Dispatch;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace fbognini.Notifications.Tests;

public class DispatcherTests
{
    private static NotificationDispatcher Build(IEnumerable<INotificationSink> sinks, DispatcherOptions? options = null)
        => new(
            sinks,
            options ?? new DispatcherOptions { BaseDelay = TimeSpan.Zero },
            NullLogger<NotificationDispatcher>.Instance,
            TimeProvider.System);

    private static NotificationRequest Request(params Recipient[] recipients)
        => NotificationRequest.To("PROFILE", "hello", recipients);

    [Fact]
    public async Task Each_sink_only_sees_recipients_of_its_own_channel()
    {
        var email = new RecordingSink("email", "email");
        var telegram = new RecordingSink("telegram", "telegram");

        await Build([email, telegram]).DispatchAsync(Request(
            Recipient.For("email", "a@b.c"),
            Recipient.For("email", "d@e.f"),
            Recipient.For("telegram", "12345")));

        Assert.Equal(2, email.Received.Single().Recipients.Count);
        Assert.Equal("12345", telegram.Received.Single().Recipients.Single().Address);
    }

    [Fact]
    public async Task A_channel_without_a_sink_is_reported_rather_than_silently_dropped()
    {
        var report = await Build([new RecordingSink("email", "email")])
            .DispatchAsync(Request(Recipient.For("sms", "+39000")));

        var outcome = Assert.Single(report.Outcomes);
        Assert.False(outcome.Result.Success);
        Assert.Contains("No sink is registered", outcome.Result.FailureReason);
    }

    [Fact]
    public async Task Transient_failures_are_retried_up_to_the_limit()
    {
        var sink = new RecordingSink("email", "email");
        sink.Results.Enqueue(NotificationResult.TransientFailure("smtp busy"));
        sink.Results.Enqueue(NotificationResult.TransientFailure("smtp busy"));
        sink.Results.Enqueue(NotificationResult.Sent());

        var report = await Build([sink], new DispatcherOptions { MaxAttempts = 3, BaseDelay = TimeSpan.Zero })
            .DispatchAsync(Request(Recipient.For("email", "a@b.c")));

        Assert.True(report.AllSucceeded);
        Assert.Equal(3, sink.Attempts);
    }

    [Fact]
    public async Task Permanent_failures_are_not_retried()
    {
        var sink = new RecordingSink("email", "email");
        sink.Results.Enqueue(NotificationResult.PermanentFailure("mailbox does not exist"));

        var report = await Build([sink]).DispatchAsync(Request(Recipient.For("email", "a@b.c")));

        Assert.False(report.AnySucceeded);
        Assert.Equal(1, sink.Attempts);
    }

    [Fact]
    public async Task Retries_stop_at_the_configured_maximum()
    {
        var sink = new RecordingSink("email", "email");

        for (var i = 0; i < 10; i++)
        {
            sink.Results.Enqueue(NotificationResult.TransientFailure("still busy"));
        }

        var report = await Build([sink], new DispatcherOptions { MaxAttempts = 2, BaseDelay = TimeSpan.Zero })
            .DispatchAsync(Request(Recipient.For("email", "a@b.c")));

        Assert.Equal(2, sink.Attempts);
        Assert.Equal(2, report.Outcomes.Single().Attempts);
    }

    [Fact]
    public async Task One_channel_failing_does_not_stop_the_others()
    {
        var email = new RecordingSink("email", "email");
        email.Results.Enqueue(NotificationResult.PermanentFailure("mailbox does not exist"));

        var telegram = new RecordingSink("telegram", "telegram");

        var report = await Build([email, telegram]).DispatchAsync(Request(
            Recipient.For("email", "a@b.c"),
            Recipient.For("telegram", "12345")));

        Assert.False(report.AllSucceeded);
        Assert.True(report.AnySucceeded);
        Assert.Single(report.Failures);
        Assert.Single(telegram.Received);
    }

    [Fact]
    public async Task A_sink_that_throws_is_contained()
    {
        var report = await Build([new ThrowingSink()]).DispatchAsync(Request(Recipient.For("email", "a@b.c")));

        Assert.False(report.AnySucceeded);
        Assert.Contains("boom", report.Outcomes.Single().Result.FailureReason);
    }

    private sealed class ThrowingSink : INotificationSink
    {
        public string Channel => "email";

        public string Name => "throwing";

        public Task<NotificationResult> SendAsync(
            NotificationRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");
    }
}
