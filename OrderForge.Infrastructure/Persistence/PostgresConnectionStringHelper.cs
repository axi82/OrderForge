using Npgsql;

namespace OrderForge.Infrastructure.Persistence;

/// <summary>
/// When Postgres runs in Docker with a random host port (e.g. <c>64649:5432</c>), set
/// <c>ORDERFORGE_POSTGRES_HOST_PORT</c> to rewrite the port in the connection string.
/// </summary>
public static class PostgresConnectionStringHelper
{
    public const string HostPortEnvironmentVariableName = "ORDERFORGE_POSTGRES_HOST_PORT";

    public static string ApplyHostPortEnvironmentOverride(string connectionString)
    {
        var portEnv = Environment.GetEnvironmentVariable(HostPortEnvironmentVariableName);
        if (string.IsNullOrWhiteSpace(portEnv)
            || !int.TryParse(portEnv, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var port)
            || port <= 0)
        {
            return connectionString;
        }

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString) { Port = port };
            return builder.ConnectionString;
        }
        catch (ArgumentException)
        {
            return connectionString;
        }
    }
}
