using System.ComponentModel.DataAnnotations;

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

    public string ProductCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public string? CommodityCodeDescription { get; set; }

    public string? SupplierAccountCode { get; set; }

    public string? PartNumber { get; set; }

    public decimal QuantityInStock { get; set; }

    public decimal QuantityAllocated { get; set; }

    public decimal QuantityOnOrder { get; set; }

    public decimal FreeStock { get; set; }

    public string? Barcode { get; set; }

    public decimal CostPrice { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>Client request for creating a product. Validation mirrors <c>CreateProductCommandValidator</c> on the API.</summary>
public sealed class CreateProductRequest
{
    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    [MaxLength(20000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Brand { get; set; }

    [MaxLength(100)]
    public string? CommodityCodeDescription { get; set; }

    [MaxLength(50)]
    public string? SupplierAccountCode { get; set; }

    [MaxLength(100)]
    public string? PartNumber { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal QuantityInStock { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal QuantityAllocated { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal QuantityOnOrder { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal FreeStock { get; set; }

    [MaxLength(64)]
    public string? Barcode { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal CostPrice { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Trims string fields and clears optional values that are empty after trim (matches API normalization).</summary>
    public void NormalizeStringsForCreate()
    {
        Sku = Sku.Trim();
        ProductCode = ProductCode.Trim();
        Name = Name.Trim();
        ShortDescription = ShortDescription.Trim();
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        Brand = string.IsNullOrWhiteSpace(Brand) ? null : Brand.Trim();
        CommodityCodeDescription = string.IsNullOrWhiteSpace(CommodityCodeDescription)
            ? null
            : CommodityCodeDescription.Trim();
        SupplierAccountCode = string.IsNullOrWhiteSpace(SupplierAccountCode) ? null : SupplierAccountCode.Trim();
        PartNumber = string.IsNullOrWhiteSpace(PartNumber) ? null : PartNumber.Trim();
        Barcode = string.IsNullOrWhiteSpace(Barcode) ? null : Barcode.Trim();
    }
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
