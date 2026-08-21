using fbognini.Notifications.Abstractions;
using fbognini.Notifications.Configuration;
using fbognini.Notifications.Sinks.MTarget;
using fbognini.Notifications.Sources.AppSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace fbognini.Notifications.Tests;

public class MTargetTests
{
    private static IConfiguration Profile() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Notifications:mtarget:SUPPORT:Username"] = "user",
            ["Notifications:mtarget:SUPPORT:Password"] = "secret",
            ["Notifications:mtarget:SUPPORT:Sender"] = "EXAMPLE",
        })
        .Build();

    private static ServiceProvider Compose()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddNotifications()
            .AddMTarget()
            .FromAppSettings(Profile());

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// The channel is the provider, not the medium: a second SMS provider gets its own channel and its
    /// own identity shape rather than colliding with this one on the same configuration key.
    /// </summary>
    [Fact]
    public void The_channel_is_named_after_the_provider()
    {
        Assert.Equal("mtarget", MTargetChannel.Name);

        var sink = Compose().GetServices<INotificationSink>().Single();

        Assert.Equal("mtarget", sink.Channel);
        Assert.Equal("mtarget", sink.Name);
    }

    [Fact]
    public async Task Its_profiles_are_read_from_the_mtarget_section()
    {
        var identity = await Compose()
            .GetRequiredService<INotificationConfigurationProvider>()
            .GetAsync<MTargetIdentity>("mtarget", "SUPPORT");

        Assert.Equal("user", identity.Username);
        Assert.Equal("EXAMPLE", identity.Sender);
    }

    [Fact]
    public async Task A_composition_with_only_mtarget_starts()
    {
        var provider = Compose();

        foreach (var hosted in provider.GetServices<IHostedService>())
        {
            await hosted.StartAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task It_ignores_recipients_addressed_to_other_channels()
    {
        var sink = Compose().GetServices<INotificationSink>().Single();

        var result = await sink.SendAsync(NotificationRequest.To(
            "SUPPORT",
            "hello",
            Recipient.For("email", "a@b.c")));

        Assert.True(result.Success);
        Assert.Contains("No MTarget recipients", result.FailureReason);
    }
}
