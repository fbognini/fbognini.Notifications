namespace fbognini.Notifications.Abstractions;

/// <summary>
/// The smallest payload every channel can honour. Anything a single channel cannot express — a subject
/// line, cc/bcc, inline keyboards — belongs to that channel's own rich interface, not here.
/// </summary>
public sealed class NotificationRequest
{
    public required string ConfigurationId { get; init; }

    public required IReadOnlyList<Recipient> Recipients { get; init; }

    public required string Body { get; init; }

    public static NotificationRequest To(string configurationId, string body, params Recipient[] recipients)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationId);
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(recipients);

        if (recipients.Length == 0)
        {
            throw new ArgumentException("At least one recipient is required.", nameof(recipients));
        }

        return new NotificationRequest
        {
            ConfigurationId = configurationId,
            Body = body,
            Recipients = recipients,
        };
    }

    public NotificationRequest ForChannel(string channel)
    {
        return new NotificationRequest
        {
            ConfigurationId = ConfigurationId,
            Body = Body,
            Recipients = Recipients.Where(r => r.Channel == channel).ToArray(),
        };
    }
}
