namespace fbognini.Notifications.Templates;

/// <summary>
/// Optional source capability. A source that does not provide templates simply does not register this,
/// and the failure surfaces at startup rather than on the first send.
/// </summary>
public interface ITemplateStore
{
    Task<NotificationTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<NotificationTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}

public sealed record NotificationTemplate(string Id, string Name, string Subject, string Body);
