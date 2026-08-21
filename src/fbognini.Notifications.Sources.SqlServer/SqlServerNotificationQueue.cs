using Dapper;
using fbognini.Notifications.Queue;
using Microsoft.Data.SqlClient;

namespace fbognini.Notifications.Sources.SqlServer;

internal sealed class SqlServerNotificationQueue(SqlServerSourceOptions options) : INotificationQueue
{
    public async Task<int> EnqueueAsync(
        IReadOnlyList<QueuedNotification> notifications,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        if (notifications.Count == 0)
        {
            return 0;
        }

        var sql = $"""
            INSERT INTO [{options.Schema}].[{options.QueueTable}]
                (Channel, ConfigurationId, Address, Payload, CreatedAt, Processing, ErrorRetry)
            VALUES (@Channel, @ConfigurationId, @Address, @Payload, @CreatedAt, 0, 0)
            """;

        var parameters = notifications
            .Select(n => new
            {
                n.Channel,
                n.ConfigurationId,
                n.Address,
                n.Payload,
                CreatedAt = n.CreatedAt == default ? DateTimeOffset.UtcNow : n.CreatedAt,
            })
            .ToArray();

        await using var connection = new SqlConnection(options.ConnectionString);

        return await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }
}
