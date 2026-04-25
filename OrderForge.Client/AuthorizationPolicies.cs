namespace OrderForge.Client;

public static class AuthorizationPolicies
{
    public const string SupplierAdmin = "SupplierAdmin";

    public const string SupplierStaff = "SupplierStaff";

    public const string InviteUsers = "InviteUsers";

    public const string Customer = "Customer";

    /// <summary>Trade portal users (Customer / CompanyAdmin), not supplier staff.</summary>
    public const string TradeAccount = "TradeAccount";
}
