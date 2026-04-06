namespace OrderForge.Application.Organisations;

public sealed record OrganisationDto(
    int Id,
    string? KeycloakOrganizationId,
    string Name,
    string? TradingAs,
    string? CompanyNumber,
    string? VatNumber,
    string? AccountNumber,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
