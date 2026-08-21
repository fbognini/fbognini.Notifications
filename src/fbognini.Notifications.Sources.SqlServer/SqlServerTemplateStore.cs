using Dapper;
using fbognini.Notifications.Templates;
using Microsoft.Data.SqlClient;

namespace fbognini.Notifications.Sources.SqlServer;

internal sealed class SqlServerTemplateStore(SqlServerSourceOptions options) : ITemplateStore
{
    public Task<NotificationTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => QueryAsync("Name = @value", name, cancellationToken);

    public Task<NotificationTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => QueryAsync("Id = @value", id, cancellationToken);

    private async Task<NotificationTemplate?> QueryAsync(
        string predicate,
        string value,
        CancellationToken cancellationToken)
    {
        var sql = $"SELECT Id, Name, Subject, Body FROM [{options.Schema}].[{options.TemplatesTable}] WHERE {predicate}";

        await using var connection = new SqlConnection(options.ConnectionString);

        return await connection.QuerySingleOrDefaultAsync<NotificationTemplate>(
            new CommandDefinition(sql, new { value }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }
}
