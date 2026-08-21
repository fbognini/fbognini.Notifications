namespace fbognini.Notifications.Abstractions;

/// <summary>
/// Optional source capability that turns a person into channel addresses. It exists because a Telegram
/// chat id is not derivable from a user the way an email address usually is, and because "which channels
/// does this user want" is the same question asked once.
/// </summary>
public interface IRecipientDirectory
{
    Task<IReadOnlyList<Recipient>> ResolveAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
