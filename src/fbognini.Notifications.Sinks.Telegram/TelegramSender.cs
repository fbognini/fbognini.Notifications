using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using fbognini.Notifications.Configuration;

namespace fbognini.Notifications.Sinks.Telegram;

internal sealed class TelegramSender(
    HttpClient client,
    INotificationConfigurationProvider configuration,
    TelegramSinkOptions options) : ITelegramSender
{
    public async Task<TelegramSendResult> SendAsync(
        TelegramMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var identity = await configuration
            .GetAsync<TelegramIdentity>(TelegramChannel.Name, message.ConfigurationId, cancellationToken)
            .ConfigureAwait(false);

        var payload = new SendMessageRequest
        {
            ChatId = message.ChatId,
            Text = message.Text,
            ParseMode = Format(message.ParseMode ?? options.ParseMode),
            DisableNotification = message.DisableNotification ?? options.DisableNotification,
            ReplyToMessageId = message.ReplyToMessageId,
        };

        var url = $"{options.ApiBaseUrl.TrimEnd('/')}/bot{identity.BotToken}/sendMessage";

        using var response = await client.PostAsJsonAsync(url, payload, cancellationToken).ConfigureAwait(false);

        var body = await response.Content
            .ReadFromJsonAsync<SendMessageResponse>(cancellationToken)
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode && body?.Ok == true)
        {
            return new TelegramSendResult(true, body.Result?.MessageId, null, null);
        }

        var retryAfter = ResolveRetryAfter(response, body);
        var error = body?.Description ?? $"Telegram returned {(int)response.StatusCode}.";

        return new TelegramSendResult(false, null, error, retryAfter);
    }

    // Telegram answers 429 with the wait it expects; honouring it is the difference between backing off
    // and being throttled harder.
    private static TimeSpan? ResolveRetryAfter(HttpResponseMessage response, SendMessageResponse? body)
    {
        if (body?.Parameters?.RetryAfter is { } seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        if (response.StatusCode is HttpStatusCode.TooManyRequests)
        {
            return response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
        }

        return null;
    }

    private static string? Format(TelegramParseMode mode) => mode switch
    {
        TelegramParseMode.Markdown => "Markdown",
        TelegramParseMode.MarkdownV2 => "MarkdownV2",
        TelegramParseMode.Html => "HTML",
        _ => null,
    };

    private sealed class SendMessageRequest
    {
        [JsonPropertyName("chat_id")]
        public required string ChatId { get; init; }

        [JsonPropertyName("text")]
        public required string Text { get; init; }

        [JsonPropertyName("parse_mode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ParseMode { get; init; }

        [JsonPropertyName("disable_notification")]
        public bool DisableNotification { get; init; }

        [JsonPropertyName("reply_to_message_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? ReplyToMessageId { get; init; }
    }

    private sealed class SendMessageResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("result")]
        public SentMessage? Result { get; init; }

        [JsonPropertyName("parameters")]
        public ResponseParameters? Parameters { get; init; }
    }

    private sealed class SentMessage
    {
        [JsonPropertyName("message_id")]
        public long MessageId { get; init; }
    }

    private sealed class ResponseParameters
    {
        [JsonPropertyName("retry_after")]
        public int? RetryAfter { get; init; }
    }
}
