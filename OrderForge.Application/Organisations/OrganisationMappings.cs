using OrderForge.Domain.Organisations;

namespace OrderForge.Application.Organisations;

internal static class OrganisationMappings
{
    public static OrganisationDto ToDto(this Organisation o) =>
        new(
            o.Id,
            o.Name,
            o.TradingAs,
            o.CompanyNumber,
            o.VatNumber,
            o.AccountNumber,
            o.Status,
            o.CreatedAt,
            o.UpdatedAt);
}
