namespace OrderForge.Application.Admin;

public static class KnownRealmRoles
{
    public const string SupplierAdmin = "SupplierAdmin";

    public const string SupplierViewer = "SupplierViewer";

    public const string CompanyAdmin = "CompanyAdmin";

    public const string Customer = "Customer";

    /// <summary>Alternate Keycloak realm role for trade portal users (synonym for trader / <see cref="Customer"/> in some realms).</summary>
    public const string TradeAccount = "TradeAccount";

    public static readonly string[] SupplierRoles = [SupplierAdmin, SupplierViewer];

    public static readonly string[] CompanyInviteRoles = [CompanyAdmin, Customer, TradeAccount];

    public static readonly string[] SupplierCreateRoles = [SupplierAdmin, SupplierViewer];
}
