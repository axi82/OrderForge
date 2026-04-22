using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence;

/// <summary>
/// Inserts sample rows for local development. Safe to call repeatedly; skips individual steps when data already exists.
/// </summary>
public static class DevelopmentDataSeeder
{
    /// <param name="importDevelopmentProductCsv">
    /// When true, loads the embedded sample product CSV if the products table is empty (local Development or Aspire).
    /// </param>
    public static async Task SeedAsync(
        OrderForgeDbContext db,
        bool importDevelopmentProductCsv,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!await db.Organisations.AnyAsync(o => o.AccountNumber == "DEV-001", cancellationToken)
                .ConfigureAwait(false))
        {
            var now = DateTime.UtcNow;
            db.Organisations.AddRange(
                new Organisation
                {
                    Name = "Acme Supplies Ltd",
                    TradingAs = "Acme Trade",
                    CompanyNumber = "12345678",
                    VatNumber = "GB123456789",
                    AccountNumber = "DEV-001",
                    OrganisationStatusId = OrganisationStatus.ActiveId,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Organisation
                {
                    Name = "Northwind Workshop",
                    TradingAs = null,
                    CompanyNumber = null,
                    VatNumber = null,
                    AccountNumber = "DEV-002",
                    OrganisationStatusId = OrganisationStatus.ActiveId,
                    CreatedAt = now,
                    UpdatedAt = now
                });

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importDevelopmentProductCsv)
        {
            await DevelopmentProductCsvSeeder
                .TrySeedFromEmbeddedCsvAsync(db, logger, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
