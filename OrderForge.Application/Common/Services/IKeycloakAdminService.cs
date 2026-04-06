namespace OrderForge.Application.Common.Services;

public interface IKeycloakAdminService
{
    Task<CreateKeycloakUserResult> CreateUserAsync(CreateKeycloakUserRequest request, CancellationToken cancellationToken = default);

    Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

    Task<KeycloakOrganizationResult> CreateOrganizationAsync(string organizationName, string? displayName, CancellationToken cancellationToken = default);

    Task AddUserToOrganizationAsync(string organizationId, string userId, CancellationToken cancellationToken = default);

    Task<KeycloakOrganizationResult?> GetOrganizationByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the user, assigns the realm role, and adds membership to the Keycloak organization.
    /// Rolls back user creation if a later step fails.
    /// </summary>
    Task<CreateKeycloakUserResult> InviteUserToCompanyAsync(InviteUserToCompanyRequest request, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}
