using fbognini.Notifications.Abstractions;
using fbognini.Notifications.Dispatch;
using fbognini.Notifications.Sinks.Email;

namespace fbognini.Notifications.WorkerSample;

/// <summary>
/// Injecting the sender into a singleton BackgroundService is safe now: nothing on it holds a profile,
/// so two tenants sending concurrently cannot end up with each other's credentials.
/// </summary>
public sealed class Worker(
    ILogger<Worker> logger,
    IEmailSender email,
    INotificationDispatcher dispatcher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SendWithTheRichInterfaceAsync(stoppingToken);
        await FanOutAcrossChannelsAsync(stoppingToken);
        await SendAsTwoTenantsConcurrentlyAsync(stoppingToken);
    }

    private async Task SendWithTheRichInterfaceAsync(CancellationToken cancellationToken)
    {
        await email.SendAsync(
            new EmailMessage
            {
                ConfigurationId = "SUPPORT",
                To = "someone@example.com",
                Cc = "archive@example.com",
                Subject = "Welcome",
                Body = "<p>Glad to have you.</p>",
                IsHtml = true,
            },
            cancellationToken);

        logger.LogInformation("Sent one email through the channel's own interface.");
    }

    private async Task FanOutAcrossChannelsAsync(CancellationToken cancellationToken)
    {
        var report = await dispatcher.DispatchAsync(
            NotificationRequest.To(
                "SUPPORT",
                "Your order has shipped.",
                Recipient.For("email", "someone@example.com"),
                Recipient.For("telegram", "123456789")),
            cancellationToken);

        foreach (var outcome in report.Outcomes)
        {
            logger.LogInformation(
                "{Channel}/{Sink}: {Status} after {Attempts} attempt(s). {Reason}",
                outcome.Channel,
                outcome.SinkName,
                outcome.Result.Success ? "sent" : "failed",
                outcome.Attempts,
                outcome.Result.FailureReason);
        }
    }

    private async Task SendAsTwoTenantsConcurrentlyAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            SendAsAsync("SUPPORT", cancellationToken),
            SendAsAsync("BILLING", cancellationToken));
    }

    private Task SendAsAsync(string profile, CancellationToken cancellationToken)
        => email.SendAsync(
            new EmailMessage
            {
                ConfigurationId = profile,
                To = "someone@example.com",
                Subject = $"Sent as {profile}",
                Body = "Each call resolves its own profile.",
            },
            cancellationToken);
}
