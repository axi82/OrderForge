namespace OrderForge.Client.Models;

public sealed class CreateCustomerCompanyRequest
{
    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? AccountNumber { get; set; }

    public string Status { get; set; } = "Active";
}

public sealed class InviteUserRequest
{
    public int OrganisationId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string RealmRoleName { get; set; } = "Customer";

    public string? TemporaryPassword { get; set; }
}

public sealed class InviteUserResponse
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}
