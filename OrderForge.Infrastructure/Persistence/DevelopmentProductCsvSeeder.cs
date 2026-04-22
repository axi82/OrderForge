using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderForge.Domain.Products;

namespace OrderForge.Infrastructure.Persistence;

/// <summary>
/// Development-only import of embedded sample product CSV (RFC 4180-style quoted fields).
/// </summary>
public static class DevelopmentProductCsvSeeder
{
    private const string CreatedBySeed = "development-seed";

    public static async Task TrySeedFromEmbeddedCsvAsync(
        OrderForgeDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        await using var stream = OpenEmbeddedCsvStream();
        if (stream is null)
        {
            logger.LogWarning(
                "Embedded product sample CSV was not found (expected resource ending with ProductRecordsSample.csv).");
            return;
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var header = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        if (header is null)
        {
            logger.LogWarning("Product sample CSV is empty.");
            return;
        }

        var now = DateTime.UtcNow;
        var batch = new List<Product>();
        var lineNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = ParseCsvLine(line);
            if (fields.Count < 10)
            {
                logger.LogWarning("Skipping line {Line}: expected at least 10 columns, got {Count}.", lineNumber, fields.Count);
                continue;
            }

            var productCode = fields[0].Trim();
            if (productCode.Length == 0)
            {
                logger.LogWarning("Skipping line {Line}: empty product code.", lineNumber);
                continue;
            }

            var description = fields[1].Trim();
            var name = description.Length <= 300 ? description : description[..300];
            var longDescription = description.Length > 300 ? description : null;

            batch.Add(
                new Product
                {
                    Sku = productCode,
                    ProductCode = productCode,
                    Name = name,
                    Description = longDescription,
                    CommodityCodeDescription = NullIfEmpty(fields[3]),
                    SupplierAccountCode = NullIfEmpty(fields[4]),
                    PartNumber = NullIfEmpty(fields[5]),
                    QuantityInStock = ParseDecimal(fields[2]),
                    QuantityAllocated = ParseDecimal(fields[6]),
                    QuantityOnOrder = ParseDecimal(fields[7]),
                    FreeStock = ParseDecimal(fields[8]),
                    Barcode = NullIfEmpty(fields[9]),
                    CostPrice = 0,
                    BasePrice = 0,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = CreatedBySeed
                });
        }

        if (batch.Count == 0)
        {
            logger.LogWarning("No product rows parsed from sample CSV.");
            return;
        }

        await db.Products.AddRangeAsync(batch, cancellationToken).ConfigureAwait(false);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Seeded {Count} products from embedded development CSV.", batch.Count);
    }

    private static Stream? OpenEmbeddedCsvStream()
    {
        var assembly = typeof(DevelopmentProductCsvSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("ProductRecordsSample.csv", StringComparison.Ordinal));
        return resourceName is null ? null : assembly.GetManifestResourceStream(resourceName);
    }

    private static string? NullIfEmpty(string value)
    {
        var t = value.Trim();
        return t.Length == 0 ? null : t;
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.TryParse(
            value.Trim(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var d)
            ? d
            : 0m;
    }

    /// <summary>Parses a single CSV line; supports commas inside quoted fields and doubled quotes.</summary>
    internal static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result;
    }
}
