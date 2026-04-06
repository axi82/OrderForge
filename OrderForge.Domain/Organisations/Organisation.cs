namespace OrderForge.Domain.Organisations;

public class Organisation
{
    public int Id { get; set; }

    /// <summary>Keycloak Organization id for customer companies (null for legacy or non-linked rows).</summary>
    public string? KeycloakOrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    /// <summary>UK Companies House number.</summary>
    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    /// <summary>Internal customer account reference.</summary>
    public string? AccountNumber { get; set; }

    public int OrganisationStatusId { get; set; }

    public OrganisationStatus OrganisationStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
