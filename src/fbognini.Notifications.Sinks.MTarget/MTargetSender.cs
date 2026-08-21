using System.Net.Http.Json;
using System.Text.Json.Serialization;
using fbognini.Notifications.Configuration;

namespace fbognini.Notifications.Sinks.MTarget;

internal sealed class MTargetSender(
    HttpClient client,
    INotificationConfigurationProvider configuration,
    MTargetSinkOptions options) : IMTargetSender
{
    public async Task<MTargetSendResult> SendAsync(
        string configurationId,
        IReadOnlyList<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default)
    {
        var identity = await configuration
            .GetAsync<MTargetIdentity>(MTargetChannel.Name, configurationId, cancellationToken)
            .ConfigureAwait(false);

        var form = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", identity.Username),
            new KeyValuePair<string, string>("password", identity.Password),
            new KeyValuePair<string, string>("sender", identity.Sender),
            new KeyValuePair<string, string>("msisdn", string.Join(',', phoneNumbers)),
            new KeyValuePair<string, string>("msg", message),
        ]);

        var url = $"{options.BaseUrl.TrimEnd('/')}/messages";

        using var response = await client.PostAsync(url, form, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return new MTargetSendResult(false, $"MTarget returned {(int)response.StatusCode}.", null);
        }

        var body = await response.Content
            .ReadFromJsonAsync<SendMessagesResponse>(cancellationToken)
            .ConfigureAwait(false);

        var result = body?.Results?.FirstOrDefault();

        if (result is null)
        {
            return new MTargetSendResult(false, "MTarget returned no result.", null);
        }

        return result.Code == "-1"
            ? new MTargetSendResult(false, result.Reason, result.Ticket)
            : new MTargetSendResult(true, null, result.Ticket);
    }

    private sealed class SendMessagesResponse
    {
        [JsonPropertyName("results")]
        public List<MessageResult>? Results { get; init; }
    }

    private sealed class MessageResult
    {
        [JsonPropertyName("code")]
        public string? Code { get; init; }

        [JsonPropertyName("reason")]
        public string? Reason { get; init; }

        [JsonPropertyName("ticket")]
        public string? Ticket { get; init; }
    }
}
