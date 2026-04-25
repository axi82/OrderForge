namespace OrderForge.Application.Common;

/// <summary>
/// Authenticated user context from the HTTP request (implemented in the API host).
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }

    bool IsSupplierAdmin { get; }

    bool IsSupplierViewer { get; }

    bool IsCompanyAdmin { get; }

    bool IsCustomer { get; }

    /// <summary>Keycloak organization id from the access token (customer users).</summary>
    string? KeycloakOrganizationId { get; }
}
