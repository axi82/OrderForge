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

    /// <summary>Updates Keycloak user first and last name via Admin API (GET + PUT user).</summary>
    Task UpdateRealmUserNamesAsync(
        string userId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    /// <summary>Sets the user's password via Admin API <c>PUT .../reset-password</c>.</summary>
    Task SetRealmUserPasswordAsync(
        string userId,
        string newPassword,
        bool temporary,
        CancellationToken cancellationToken = default);

    /// <summary>Lists realm users (brief). Maps to Keycloak <c>GET .../users</c>.</summary>
    Task<IReadOnlyList<KeycloakRealmUserBrief>> SearchRealmUsersAsync(
        int first,
        int max,
        string? search,
        CancellationToken cancellationToken = default);

    /// <summary>Count of users matching Keycloak criteria (may include service accounts).</summary>
    Task<int> CountRealmUsersAsync(string? search, CancellationToken cancellationToken = default);

    /// <summary>Organization display names for a user (Keycloak Organizations).</summary>
    Task<IReadOnlyList<string>> GetOrganizationNamesForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>Latest <c>lastAccess</c> across user sessions, or null if none.</summary>
    Task<DateTime?> GetLatestSessionLastAccessUtcAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
