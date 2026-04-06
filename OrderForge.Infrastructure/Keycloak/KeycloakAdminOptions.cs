namespace OrderForge.Infrastructure.Keycloak;

public sealed class KeycloakAdminOptions
{
    public const string SectionName = "KeycloakAdmin";

    /// <summary>Keycloak base URL without trailing slash, e.g. http://localhost:8081</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string Realm { get; set; } = "orderforge";

    public string ClientId { get; set; } = "orderforge-admin-api";

    public string ClientSecret { get; set; } = string.Empty;
}
