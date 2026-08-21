using fbognini.Notifications.Abstractions;
using fbognini.Notifications.Builder;
using fbognini.Notifications.Sinks.Email;
using fbognini.Notifications.Sinks.Telegram;
using fbognini.Notifications.Sources.AppSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace fbognini.Notifications.Tests;

public class CompositionTests
{
    private static IConfiguration Configuration(params (string Key, string Value)[] entries)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(entries.Select(e => new KeyValuePair<string, string?>(e.Key, e.Value)))
            .Build();

    private static IConfiguration SupportProfile() => Configuration(
        ("Notifications:email:SUPPORT:SmtpHost", "localhost"),
        ("Notifications:email:SUPPORT:SmtpPort", "25"),
        ("Notifications:email:SUPPORT:FromEmail", "support@example.com"));

    private static async Task<Exception?> StartHostAsync(Action<NotificationsBuilder> compose)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        compose(services.AddNotifications());

        var provider = services.BuildServiceProvider();

        try
        {
            foreach (var hosted in provider.GetServices<IHostedService>())
            {
                await hosted.StartAsync(CancellationToken.None);
            }

            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    [Fact]
    public async Task A_sink_without_a_configuration_source_fails_at_startup()
    {
        var error = await StartHostAsync(b => b.AddEmail());

        var composition = Assert.IsType<NotificationCompositionException>(error);
        Assert.Contains("no configuration source", composition.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_source_without_any_sink_fails_at_startup()
    {
        var error = await StartHostAsync(b => b.FromAppSettings(SupportProfile()));

        var composition = Assert.IsType<NotificationCompositionException>(error);
        Assert.Contains("No notification sink", composition.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_complete_composition_starts()
    {
        var error = await StartHostAsync(b => b.AddEmail().FromAppSettings(SupportProfile()));

        Assert.Null(error);
    }

    [Fact]
    public async Task A_malformed_static_profile_fails_at_startup_not_on_first_send()
    {
        var configuration = Configuration(
            ("Notifications:email:SUPPORT:SmtpHost", "localhost"),
            ("Notifications:email:SUPPORT:SmtpPort", "not-a-number"),
            ("Notifications:email:SUPPORT:FromEmail", "support@example.com"));

        var error = await StartHostAsync(b => b.AddEmail().FromAppSettings(configuration));

        Assert.NotNull(error);
        Assert.Contains("SUPPORT", error!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Two_sinks_on_the_same_channel_need_distinct_names()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var builder = services.AddNotifications().AddEmail();

        Assert.Throws<InvalidOperationException>(() => builder.AddEmail());
    }

    [Fact]
    public async Task Two_sinks_on_the_same_channel_both_receive()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddNotifications()
            .AddEmail()
            .AddEmail(o => o.Name = "backup-smtp")
            .FromAppSettings(SupportProfile());

        var provider = services.BuildServiceProvider();
        var sinks = provider.GetServices<INotificationSink>().ToArray();

        Assert.Equal(2, sinks.Length);
        Assert.Equal(["email", "backup-smtp"], sinks.Select(s => s.Name));

        foreach (var hosted in provider.GetServices<IHostedService>())
        {
            await hosted.StartAsync(CancellationToken.None);
        }
    }

    [Fact]
    public void A_named_sink_can_be_resolved_by_key()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddNotifications()
            .AddEmail()
            .AddTelegram()
            .FromAppSettings(SupportProfile());

        var provider = services.BuildServiceProvider();

        Assert.Equal("telegram", provider.GetRequiredKeyedService<INotificationSink>("telegram").Channel);
        Assert.Equal("email", provider.GetRequiredKeyedService<INotificationSink>("email").Channel);
    }

    /// <summary>
    /// The point of the whole rewrite: a channel the core has never heard of composes with an existing
    /// source without a line changing anywhere else.
    /// </summary>
    [Fact]
    public async Task A_channel_the_core_does_not_know_composes_with_an_existing_source()
    {
        var configuration = Configuration(
            ("Notifications:carrier-pigeon:AVIARY:LoftAddress", "north tower"));

        var services = new ServiceCollection();
        services.AddLogging();

        var builder = services.AddNotifications().FromAppSettings(configuration);
        builder.AddSink("carrier-pigeon", "pigeon", _ => new RecordingSink("carrier-pigeon", "pigeon"));

        var provider = services.BuildServiceProvider();

        foreach (var hosted in provider.GetServices<IHostedService>())
        {
            await hosted.StartAsync(CancellationToken.None);
        }

        var resolved = await provider
            .GetRequiredService<Configuration.INotificationConfigurationProvider>()
            .GetAsync<PigeonIdentity>("carrier-pigeon", "AVIARY");

        Assert.Equal("north tower", resolved.LoftAddress);
    }

    private sealed class PigeonIdentity
    {
        public string LoftAddress { get; set; } = default!;
    }
}
