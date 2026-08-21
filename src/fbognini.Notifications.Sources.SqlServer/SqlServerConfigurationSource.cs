using Dapper;
using fbognini.Notifications.Configuration;
using Microsoft.Data.SqlClient;

namespace fbognini.Notifications.Sources.SqlServer;

/// <summary>
/// Returns the stored payload verbatim. The source never deserialises it, which is why adding a channel
/// does not touch this package.
/// </summary>
internal sealed class SqlServerConfigurationSource(SqlServerSourceOptions options) : INotificationConfigurationSource
{
    public async ValueTask<string?> ReadAsync(
        string channel,
        string id,
        CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT Payload FROM [{options.Schema}].[{options.ProfilesTable}] WHERE Channel = @channel AND Id = @id";

        await using var connection = new SqlConnection(options.ConnectionString);

        return await connection.QuerySingleOrDefaultAsync<string?>(
            new CommandDefinition(sql, new { channel, id }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }
}
