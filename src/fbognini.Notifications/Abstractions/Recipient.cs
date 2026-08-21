namespace fbognini.Notifications.Abstractions;

/// <summary>
/// A destination on a specific channel. The address is opaque to the core: an email address, a phone
/// number and a Telegram chat id all travel through this type and are interpreted by the sink alone.
/// </summary>
public sealed record Recipient(string Channel, string Address)
{
    public static Recipient For(string channel, string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        return new Recipient(channel, address);
    }
}
