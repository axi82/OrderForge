namespace OrderForge.Application.Admin;

public static class KnownRealmRoles
{
    public const string SupplierAdmin = "SupplierAdmin";

    public const string SupplierViewer = "SupplierViewer";

    public const string CompanyAdmin = "CompanyAdmin";

    public const string Customer = "Customer";

    public static readonly string[] SupplierRoles = [SupplierAdmin, SupplierViewer];

    public static readonly string[] CompanyInviteRoles = [CompanyAdmin, Customer];

    public static readonly string[] SupplierCreateRoles = [SupplierAdmin, SupplierViewer];
}
