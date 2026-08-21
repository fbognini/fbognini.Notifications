using System.Text.RegularExpressions;

namespace fbognini.Notifications.Sources.SqlServer;

public sealed class SqlServerSourceOptions
{
    public string ConnectionString { get; set; } = default!;

    public string Schema { get; set; } = "notification";

    public string ProfilesTable { get; set; } = "Profiles";

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ConnectionString);

        // These are interpolated into SQL, so they are checked rather than parameterised.
        EnsureIdentifier(Schema, nameof(Schema));
        EnsureIdentifier(ProfilesTable, nameof(ProfilesTable));
    }

    private static void EnsureIdentifier(string value, string name)
    {
        if (!Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9_]*$"))
        {
            throw new ArgumentException($"{name} must be a plain SQL identifier, got '{value}'.", name);
        }
    }
}
