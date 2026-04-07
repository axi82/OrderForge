namespace OrderForge.Application.Common.Services;

public interface IKeycloakAdminService
{
    Task<CreateKeycloakUserResult> CreateUserAsync(CreateKeycloakUserRequest request, CancellationToken cancellationToken = default);

    Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

    /// <param name="organizationInternetDomain">Email-style domain for the org (e.g. acme.com). Keycloak requires at least one; if null/empty, a placeholder is generated.</param>
    Task<KeycloakOrganizationResult> CreateOrganizationAsync(
        string organizationName,
        string? displayName,
        string? organizationInternetDomain,
        CancellationToken cancellationToken = default);

    Task AddUserToOrganizationAsync(string organizationId, string userId, CancellationToken cancellationToken = default);

    Task<KeycloakOrganizationResult?> GetOrganizationByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the user, assigns the realm role, and adds membership to the Keycloak organization.
    /// Rolls back user creation if a later step fails.
    /// </summary>
    Task<CreateKeycloakUserResult> InviteUserToCompanyAsync(InviteUserToCompanyRequest request, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}
