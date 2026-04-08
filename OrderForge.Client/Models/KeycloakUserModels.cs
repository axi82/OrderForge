namespace OrderForge.Client.Models;

/// <summary>
/// Keycloak user representation (subset of UserRepresentation used by Admin REST and OrderForge flows).
/// </summary>
public sealed class KeycloakUser
{
    public string Id { get; set; } = string.Empty;

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool Enabled { get; set; } = true;

    public bool EmailVerified { get; set; }

    /// <summary>Unix ms when the user was created, if returned by the API.</summary>
    public long? CreatedTimestamp { get; set; }
}

/// <summary>
/// Request to create a user in Keycloak (password optional; Keycloak may require actions to set password).
/// </summary>
public sealed class CreateKeycloakUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool Enabled { get; set; } = true;

    public bool EmailVerified { get; set; }

    /// <summary>If set, Keycloak creates a temporary password credential.</summary>
    public string? TemporaryPassword { get; set; }
}

/// <summary>
/// Request to update an existing Keycloak user. Only non-null fields should be applied (PATCH-style).
/// </summary>
public sealed class UpdateKeycloakUserRequest
{
    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool? Enabled { get; set; }

    public bool? EmailVerified { get; set; }

    /// <summary>If set, replaces the password (implementation-specific; may be temporary).</summary>
    public string? TemporaryPassword { get; set; }
}

/// <summary>
/// Identifies a user to remove from Keycloak. Use <see cref="UserId"/> with the Keycloak user id (UUID string).
/// </summary>
public sealed class DeleteKeycloakUserRequest
{
    public string UserId { get; set; } = string.Empty;
}
