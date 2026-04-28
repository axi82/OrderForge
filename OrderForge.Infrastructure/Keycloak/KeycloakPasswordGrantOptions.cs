namespace OrderForge.Infrastructure.Keycloak;

/// <summary>
/// OAuth client used only for server-side password verification (token endpoint grant_type=password).
/// Defaults to Keycloak's <c>admin-cli</c>, which has direct access grants enabled in the dev realm.
/// </summary>
public sealed class KeycloakPasswordGrantOptions
{
    public const string SectionName = "KeycloakPasswordGrant";

    public string ClientId { get; set; } = "admin-cli";

    /// <summary>Optional client secret when using a confidential password-grant client.</summary>
    public string? ClientSecret { get; set; }
}
