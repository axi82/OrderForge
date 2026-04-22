using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace OrderForge.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> when the API host is not running.
/// Connection resolution order:
/// <list type="number">
/// <item><c>ConnectionStrings__Default</c> environment variable</item>
/// <item><c>ConnectionStrings:Default</c> from <c>OrderForge.Api/appsettings.Development.json</c> (if present)</item>
/// <item><c>ConnectionStrings:Default</c> from <c>OrderForge.Api/appsettings.json</c></item>
/// <item>Built-in localhost default</item>
/// </list>
/// If Postgres is published on a random host port (e.g. Docker <c>59212:5432</c>), set
/// <c>ORDERFORGE_POSTGRES_HOST_PORT</c> to that host port to rewrite the connection string port.
/// </summary>
public sealed class OrderForgeDbContextFactory : IDesignTimeDbContextFactory<OrderForgeDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=orderforge;Username=orderforge;Password=orderforge";

    public OrderForgeDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<OrderForgeDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new OrderForgeDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return ApplyHostPortOverride(fromEnv.Trim());
        }

        var apiDir = LocateOrderForgeApiDirectory();
        if (apiDir is not null)
        {
            var devPath = Path.Combine(apiDir, "appsettings.Development.json");
            var fromDev = ReadConnectionStringDefault(devPath);
            if (!string.IsNullOrWhiteSpace(fromDev))
            {
                return ApplyHostPortOverride(fromDev.Trim());
            }

            var basePath = Path.Combine(apiDir, "appsettings.json");
            var fromBase = ReadConnectionStringDefault(basePath);
            if (!string.IsNullOrWhiteSpace(fromBase))
            {
                return ApplyHostPortOverride(fromBase.Trim());
            }
        }

        return ApplyHostPortOverride(DefaultConnectionString);
    }

    /// <summary>
    /// When Docker maps a host port (for example 59212) to container port 5432, host tools must use the host port.
    /// Set <c>ORDERFORGE_POSTGRES_HOST_PORT=59212</c> (or edit API appsettings) before <c>dotnet ef database update</c>.
    /// </summary>
    private static string ApplyHostPortOverride(string connectionString)
    {
        var portEnv = Environment.GetEnvironmentVariable("ORDERFORGE_POSTGRES_HOST_PORT");
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

    private static string? ReadConnectionStringDefault(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(jsonFilePath);
            using var doc = JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var cs)
                || !cs.TryGetProperty("Default", out var d))
            {
                return null;
            }

            return d.GetString();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? LocateOrderForgeApiDirectory()
    {
        foreach (var start in GetCandidateStartDirectories())
        {
            var dir = new DirectoryInfo(start);
            while (dir is not null)
            {
                var nested = Path.Combine(dir.FullName, "OrderForge.Api");
                if (File.Exists(Path.Combine(nested, "appsettings.json")))
                {
                    return nested;
                }

                if (File.Exists(Path.Combine(dir.FullName, "OrderForge.Api.csproj"))
                    && File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetCandidateStartDirectories()
    {
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;
    }
}
