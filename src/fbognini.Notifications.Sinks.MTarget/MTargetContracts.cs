namespace fbognini.Notifications.Sinks.MTarget;

public static class MTargetChannel
{
    public const string Name = "mtarget";

    public const string DefaultSinkName = "mtarget";
}

public sealed class MTargetIdentity
{
    public string Username { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string Sender { get; set; } = default!;
}

public enum MTargetEnvironment
{
    Private,
    Public,
}

public sealed class MTargetSinkOptions
{
    public string Name { get; set; } = MTargetChannel.DefaultSinkName;

    public MTargetEnvironment Environment { get; set; } = MTargetEnvironment.Private;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    internal string BaseUrl => Environment == MTargetEnvironment.Public
        ? "https://api-public-2.mtarget.fr/"
        : "https://api-2.mtarget.fr/";
}

public sealed record MTargetSendResult(bool Success, string? Error, string? Ticket);

public interface IMTargetSender
{
    Task<MTargetSendResult> SendAsync(
        string configurationId,
        IReadOnlyList<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default);
}
