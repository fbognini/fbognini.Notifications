namespace fbognini.Notifications.Sinks.Email;

/// <summary>
/// Per-profile identity, resolved on every send from the configuration provider. Never held on an
/// instance: this is what makes one sink safe to use for many tenants concurrently.
/// </summary>
public sealed class EmailIdentity
{
    public string SmtpHost { get; set; } = default!;

    public int SmtpPort { get; set; } = 25;

    public bool UseSsl { get; set; }

    public bool UseAuthentication { get; set; }

    public string? SmtpUsername { get; set; }

    public string? SmtpPassword { get; set; }

    public string FromEmail { get; set; } = default!;

    public string? FromName { get; set; }

    public string? ReplyToEmail { get; set; }
}

/// <summary>
/// Bootstrap configuration for the sink itself: fixed for the life of the process, supplied as an
/// argument to AddEmail rather than resolved per send.
/// </summary>
public sealed class EmailSinkOptions
{
    public string Name { get; set; } = EmailChannel.DefaultSinkName;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

public static class EmailChannel
{
    public const string Name = "email";

    public const string DefaultSinkName = "email";
}

public sealed class EmailMessage
{
    public required string ConfigurationId { get; init; }

    public string? To { get; init; }

    public string? Cc { get; init; }

    public string? Bcc { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }

    public bool IsHtml { get; init; }

    public IReadOnlyList<string>? Attachments { get; init; }
}

/// <summary>
/// The channel's rich interface. Callers that need cc, attachments or a subject take a dependency on
/// this; callers that only need "notify someone" go through INotificationDispatcher instead.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

    Task<int> ScheduleAsync(IReadOnlyList<EmailMessage> messages, CancellationToken cancellationToken = default);
}
