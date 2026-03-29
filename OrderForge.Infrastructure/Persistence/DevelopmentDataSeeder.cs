using Microsoft.EntityFrameworkCore;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence;

/// <summary>
/// Inserts sample rows for local development. Safe to call repeatedly; skips when seed data already exists.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(OrderForgeDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Organisations.AnyAsync(o => o.AccountNumber == "DEV-001", cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        db.Organisations.AddRange(
            new Organisation
            {
                Name = "Acme Supplies Ltd",
                TradingAs = "Acme Trade",
                CompanyNumber = "12345678",
                VatNumber = "GB123456789",
                AccountNumber = "DEV-001",
                Status = "Active",
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
                Status = "Active",
                CreatedAt = now,
                UpdatedAt = now
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}
