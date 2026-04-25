namespace OrderForge.Client;

public static class Routes
{
    public static class App
    {
        public const string Root = "";
    }

    public static class Auth
    {
        public const string SignIn = "auth/login";
        public const string OidcLogin = "authentication/login";
        public const string OidcLogout = "authentication/logout";
        public const string LoginFailed = "auth/login-failed";

        public static string SignInWithReturnUrl(string returnUrl) =>
            $"{SignIn}?returnUrl={Uri.EscapeDataString(returnUrl)}";

        public static string OidcLoginWithReturnUrl(string returnUrl) =>
            $"{OidcLogin}?returnUrl={Uri.EscapeDataString(returnUrl)}";
    }

    public static class Store
    {
        public const string Catalog = "catalog";
        public const string Cart = "cart";
        public const string Orders = "orders";

        public static string OrderDetail(int orderId) => $"{Orders}/{orderId}";
    }

    public static class Admin
    {
        public const string Companies = "admin/companies";
        public const string CompaniesCreate = "admin/companies/create";
        public const string Users = "admin/users";
        public const string UsersCreate = "admin/users/create";
        public const string UsersInvite = "admin/users/invite";

        public const string Products = "admin/products";
    }

    public static class Supplier
    {
        public const string OrganisationsTest = "organisations-test";
    }

    public static class Nav
    {
        public const string SignIn = "login";
    }
}
