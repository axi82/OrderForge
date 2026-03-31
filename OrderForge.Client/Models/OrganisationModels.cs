namespace OrderForge.Client.Models;

public sealed class OrganisationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? AccountNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateOrganisationRequest
{
    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? AccountNumber { get; set; }

    public string Status { get; set; } = "Active";
}

public sealed class UpdateOrganisationRequest
{
    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? AccountNumber { get; set; }

    public string Status { get; set; } = string.Empty;
}
