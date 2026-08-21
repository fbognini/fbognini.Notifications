namespace fbognini.Notifications.Sinks.Telegram;

public static class TelegramChannel
{
    public const string Name = "telegram";

    public const string DefaultSinkName = "telegram";
}

/// <summary>
/// Per-profile identity. A bot token is a secret on the same footing as an SMTP password, and belongs
/// in the same layer.
/// </summary>
public sealed class TelegramIdentity
{
    public string BotToken { get; set; } = default!;
}

public sealed class TelegramSinkOptions
{
    public string Name { get; set; } = TelegramChannel.DefaultSinkName;

    public string ApiBaseUrl { get; set; } = "https://api.telegram.org";

    public TelegramParseMode ParseMode { get; set; } = TelegramParseMode.None;

    public bool DisableNotification { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

public enum TelegramParseMode
{
    None,
    Markdown,
    MarkdownV2,
    Html,
}

/// <summary>
/// A Telegram destination is a chat id, not an address: it exists only once the user has started the
/// bot. Resolving a person to one is the job of IRecipientDirectory, not of this message.
/// </summary>
public sealed class TelegramMessage
{
    public required string ConfigurationId { get; init; }

    public required string ChatId { get; init; }

    public required string Text { get; init; }

    public TelegramParseMode? ParseMode { get; init; }

    public bool? DisableNotification { get; init; }

    public long? ReplyToMessageId { get; init; }
}

public sealed record TelegramSendResult(bool Success, long? MessageId, string? Error, TimeSpan? RetryAfter);

public interface ITelegramSender
{
    Task<TelegramSendResult> SendAsync(TelegramMessage message, CancellationToken cancellationToken = default);
}
