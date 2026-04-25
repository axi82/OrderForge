using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderForge.Domain.Organisations;
using OrderForge.Domain.Orders;

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

        if (!await db.TradeOrders.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var acme = await db.Organisations.AsNoTracking()
                .FirstOrDefaultAsync(o => o.AccountNumber == "DEV-001", cancellationToken)
                .ConfigureAwait(false);
            var northwind = await db.Organisations.AsNoTracking()
                .FirstOrDefaultAsync(o => o.AccountNumber == "DEV-002", cancellationToken)
                .ConfigureAwait(false);

            if (acme is not null)
            {
                var orderA = new TradeOrder
                {
                    OrganisationId = acme.Id,
                    OrderNumber = "OF-10492",
                    PlacedAt = new DateTime(2026, 4, 1, 14, 30, 0, DateTimeKind.Utc),
                    Status = "Shipped",
                    Total = 1280.40m,
                };
                orderA.Lines.Add(
                    new TradeOrderLine
                    {
                        SortOrder = 1,
                        Description = "Heavy-duty hinge set (10 pack)",
                        Quantity = 4,
                        LineTotal = 320.00m,
                    });
                orderA.Lines.Add(
                    new TradeOrderLine
                    {
                        SortOrder = 2,
                        Description = "Galvanised coach bolts M10 × 80",
                        Quantity = 200,
                        LineTotal = 480.40m,
                    });
                orderA.Lines.Add(
                    new TradeOrderLine
                    {
                        SortOrder = 3,
                        Description = "Workshop consumables bundle",
                        Quantity = 1,
                        LineTotal = 480.00m,
                    });
                db.TradeOrders.Add(orderA);

                var orderB = new TradeOrder
                {
                    OrganisationId = acme.Id,
                    OrderNumber = "OF-10471",
                    PlacedAt = new DateTime(2026, 3, 28, 9, 15, 0, DateTimeKind.Utc),
                    Status = "Processing",
                    Total = 412.00m,
                };
                orderB.Lines.Add(
                    new TradeOrderLine
                    {
                        SortOrder = 1,
                        Description = "PPE starter kit",
                        Quantity = 2,
                        LineTotal = 412.00m,
                    });
                db.TradeOrders.Add(orderB);
            }

            if (northwind is not null)
            {
                var orderN = new TradeOrder
                {
                    OrganisationId = northwind.Id,
                    OrderNumber = "OF-20001",
                    PlacedAt = new DateTime(2026, 4, 2, 11, 0, 0, DateTimeKind.Utc),
                    Status = "Pending",
                    Total = 99.00m,
                };
                orderN.Lines.Add(
                    new TradeOrderLine
                    {
                        SortOrder = 1,
                        Description = "Sample line (other company)",
                        Quantity = 1,
                        LineTotal = 99.00m,
                    });
                db.TradeOrders.Add(orderN);
            }

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
