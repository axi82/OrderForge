namespace OrderForge.Api;

public static class AuthorizationPolicies
{
    public const string SupplierAdmin = "SupplierAdmin";

    public const string SupplierStaff = "SupplierStaff";

    public const string InviteUsers = "InviteUsers";

    /// <summary>Trade portal: Customer (trader) or CompanyAdmin with organisation scope.</summary>
    public const string TradeAccount = "TradeAccount";
}
