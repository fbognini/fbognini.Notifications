using fbognini.Notifications.Configuration;
using fbognini.Notifications.Sinks.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace fbognini.Notifications.Tests;

public class ConfigurationResolutionTests
{
    private static LayeredConfigurationProvider Build(
        IStaticConfigurationStore? statics = null,
        INotificationConfigurationSource? dynamics = null,
        NotificationConfigurationOptions? options = null,
        TimeProvider? time = null)
        => new(
            options ?? new NotificationConfigurationOptions(),
            NullLogger<LayeredConfigurationProvider>.Instance,
            time ?? TimeProvider.System,
            statics,
            dynamics);

    [Fact]
    public async Task Static_profiles_win_over_dynamic_ones()
    {
        var statics = new FakeStaticStore();
        statics.Set("email", "SUPPORT", new EmailIdentity { SmtpHost = "static-host", FromEmail = "a@b.c" });

        var dynamics = new FakeDynamicSource();
        dynamics.Set("email", "SUPPORT", new EmailIdentity { SmtpHost = "db-host", FromEmail = "a@b.c" });

        var identity = await Build(statics, dynamics).GetAsync<EmailIdentity>("email", "SUPPORT");

        Assert.Equal("static-host", identity.SmtpHost);
        Assert.Equal(0, dynamics.Reads);
    }

    [Fact]
    public async Task Unknown_id_throws_instead_of_falling_back()
    {
        var provider = Build(dynamics: new FakeDynamicSource());

        await Assert.ThrowsAsync<NotificationConfigurationNotFoundException>(
            async () => await provider.GetAsync<EmailIdentity>("email", "NOPE"));
    }

    [Fact]
    public async Task Dynamic_reads_are_cached_until_the_ttl_lapses()
    {
        var time = new FakeTimeProvider();
        var dynamics = new FakeDynamicSource();
        dynamics.Set("email", "T1", new EmailIdentity { SmtpHost = "h", FromEmail = "a@b.c" });

        var options = new NotificationConfigurationOptions { DynamicCacheTtl = TimeSpan.FromMinutes(5) };
        var provider = Build(dynamics: dynamics, options: options, time: time);

        await provider.GetAsync<EmailIdentity>("email", "T1");
        await provider.GetAsync<EmailIdentity>("email", "T1");
        Assert.Equal(1, dynamics.Reads);

        time.Advance(TimeSpan.FromMinutes(6));
        await provider.GetAsync<EmailIdentity>("email", "T1");
        Assert.Equal(2, dynamics.Reads);
    }

    [Fact]
    public async Task Invalidate_forces_the_next_read_to_hit_the_source()
    {
        var dynamics = new FakeDynamicSource();
        dynamics.Set("email", "T1", new EmailIdentity { SmtpHost = "first", FromEmail = "a@b.c" });

        var provider = Build(dynamics: dynamics);
        await provider.GetAsync<EmailIdentity>("email", "T1");

        dynamics.Set("email", "T1", new EmailIdentity { SmtpHost = "second", FromEmail = "a@b.c" });
        provider.Invalidate("email", "T1");

        var identity = await provider.GetAsync<EmailIdentity>("email", "T1");

        Assert.Equal("second", identity.SmtpHost);
        Assert.Equal(2, dynamics.Reads);
    }

    [Fact]
    public async Task A_failing_source_serves_the_last_known_value()
    {
        var time = new FakeTimeProvider();
        var dynamics = new FakeDynamicSource();
        dynamics.Set("email", "T1", new EmailIdentity { SmtpHost = "known", FromEmail = "a@b.c" });

        var options = new NotificationConfigurationOptions { DynamicCacheTtl = TimeSpan.FromMinutes(1) };
        var provider = Build(dynamics: dynamics, options: options, time: time);

        await provider.GetAsync<EmailIdentity>("email", "T1");

        time.Advance(TimeSpan.FromMinutes(5));
        dynamics.ThrowOnRead = new InvalidOperationException("database is down");

        var identity = await provider.GetAsync<EmailIdentity>("email", "T1");

        Assert.Equal("known", identity.SmtpHost);
    }

    [Fact]
    public async Task A_failing_source_throws_when_nothing_was_ever_cached()
    {
        var dynamics = new FakeDynamicSource { ThrowOnRead = new InvalidOperationException("database is down") };
        var provider = Build(dynamics: dynamics);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await provider.GetAsync<EmailIdentity>("email", "T1"));
    }

    [Fact]
    public async Task Serving_stale_can_be_turned_off()
    {
        var time = new FakeTimeProvider();
        var dynamics = new FakeDynamicSource();
        dynamics.Set("email", "T1", new EmailIdentity { SmtpHost = "known", FromEmail = "a@b.c" });

        var options = new NotificationConfigurationOptions
        {
            DynamicCacheTtl = TimeSpan.FromMinutes(1),
            ServeStaleOnSourceFailure = false,
        };

        var provider = Build(dynamics: dynamics, options: options, time: time);
        await provider.GetAsync<EmailIdentity>("email", "T1");

        time.Advance(TimeSpan.FromMinutes(5));
        dynamics.ThrowOnRead = new InvalidOperationException("database is down");

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await provider.GetAsync<EmailIdentity>("email", "T1"));
    }

    /// <summary>
    /// The failure this guards against is sending with another tenant's credentials, so it is checked
    /// with ids that a concatenated cache key would have merged.
    /// </summary>
    [Theory]
    [InlineData("email", "A", "emai", "lA")]
    [InlineData("sms", "1", "sm", "s1")]
    public async Task Profiles_never_collide_on_a_cache_key(
        string channelOne,
        string idOne,
        string channelTwo,
        string idTwo)
    {
        var dynamics = new FakeDynamicSource();
        dynamics.Set(channelOne, idOne, new EmailIdentity { SmtpHost = "first", FromEmail = "a@b.c" });
        dynamics.Set(channelTwo, idTwo, new EmailIdentity { SmtpHost = "second", FromEmail = "a@b.c" });

        var provider = Build(dynamics: dynamics);

        var first = await provider.GetAsync<EmailIdentity>(channelOne, idOne);
        var second = await provider.GetAsync<EmailIdentity>(channelTwo, idTwo);

        Assert.Equal("first", first.SmtpHost);
        Assert.Equal("second", second.SmtpHost);
    }

    /// <summary>
    /// This is the test that fails on 2.x, where ChangeId mutated a shared instance.
    /// </summary>
    [Fact]
    public async Task Concurrent_profiles_do_not_contaminate_each_other()
    {
        var dynamics = new FakeDynamicSource();

        for (var i = 0; i < 50; i++)
        {
            dynamics.Set("email", $"TENANT{i}", new EmailIdentity
            {
                SmtpHost = $"host-{i}",
                FromEmail = $"tenant{i}@example.com",
            });
        }

        var provider = Build(dynamics: dynamics);

        var work = Enumerable.Range(0, 50).SelectMany(i => Enumerable.Repeat(i, 20)).Select(async i =>
        {
            var identity = await provider.GetAsync<EmailIdentity>("email", $"TENANT{i}");

            Assert.Equal($"host-{i}", identity.SmtpHost);
            Assert.Equal($"tenant{i}@example.com", identity.FromEmail);
        });

        await Task.WhenAll(work);
    }

    [Fact]
    public async Task The_dynamic_cache_stays_bounded()
    {
        var dynamics = new FakeDynamicSource();
        var options = new NotificationConfigurationOptions { DynamicCacheCapacity = 20 };
        var provider = Build(dynamics: dynamics, options: options);

        for (var i = 0; i < 200; i++)
        {
            dynamics.Set("email", $"T{i}", new EmailIdentity { SmtpHost = $"h{i}", FromEmail = "a@b.c" });
            await provider.GetAsync<EmailIdentity>("email", $"T{i}");
        }

        Assert.True(provider.CachedEntryCount <= 20, $"cache grew to {provider.CachedEntryCount}");
    }
}
