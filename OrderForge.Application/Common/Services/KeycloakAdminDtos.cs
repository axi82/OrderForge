namespace OrderForge.Application.Common.Services;

public sealed record CreateKeycloakUserRequest(
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool Enabled,
    string? TemporaryPassword,
    bool EmailVerified = false);

/// <summary>High-level invite flow inputs (orchestrated by <see cref="IKeycloakAdminService"/>).</summary>
public sealed record InviteUserToCompanyRequest(
    string KeycloakOrganizationId,
    string Email,
    string? FirstName,
    string? LastName,
    string RealmRoleName,
    string? TemporaryPassword);

public sealed record CreateKeycloakUserResult(string UserId, string Username);

public sealed record KeycloakOrganizationResult(string Id, string Name);
