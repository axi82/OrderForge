using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
/// If Postgres is published on a random host port (e.g. Docker <c>64649:5432</c>), set
/// <see cref="PostgresConnectionStringHelper.HostPortEnvironmentVariableName"/> to that host port to rewrite the port,
/// or set <c>ConnectionStrings:Default</c> in <c>appsettings.Development.json</c> (see API project).
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
            return PostgresConnectionStringHelper.ApplyHostPortEnvironmentOverride(fromEnv.Trim());
        }

        var apiDir = LocateOrderForgeApiDirectory();
        if (apiDir is not null)
        {
            var devPath = Path.Combine(apiDir, "appsettings.Development.json");
            var fromDev = ReadConnectionStringDefault(devPath);
            if (!string.IsNullOrWhiteSpace(fromDev))
            {
                return PostgresConnectionStringHelper.ApplyHostPortEnvironmentOverride(fromDev.Trim());
            }

            var basePath = Path.Combine(apiDir, "appsettings.json");
            var fromBase = ReadConnectionStringDefault(basePath);
            if (!string.IsNullOrWhiteSpace(fromBase))
            {
                return PostgresConnectionStringHelper.ApplyHostPortEnvironmentOverride(fromBase.Trim());
            }
        }

        return PostgresConnectionStringHelper.ApplyHostPortEnvironmentOverride(DefaultConnectionString);
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
