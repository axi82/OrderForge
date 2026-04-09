namespace OrderForge.Client.Models;

public sealed class CreateCustomerCompanyRequest
{
    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? AccountNumber { get; set; }

    /// <summary>Email-style domain for Keycloak organisation (e.g. acme.com). If empty, a placeholder is generated.</summary>
    public string? OrganizationDomain { get; set; }

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

public sealed class AdminUsersListResult
{
    public List<AdminUserRow> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string? Search { get; set; }
}

public sealed class AdminProductsListResult
{
    public List<ProductDto> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string? Search { get; set; }
}

public sealed class ProductDto
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public decimal CostPrice { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}

public sealed class CreateProductRequest
{
    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public decimal CostPrice { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class AdminUserRow
{
    public string Id { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public bool Enabled { get; set; }

    public string OrganisationNames { get; set; } = string.Empty;

    public DateTime? LastLoginUtc { get; set; }
}
