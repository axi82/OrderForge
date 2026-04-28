namespace OrderForge.Application.Common.Services;

/// <summary>
/// Validates a user's current password against Keycloak (resource-owner password grant).
/// </summary>
public interface IKeycloakUserPasswordValidator
{
    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the password is wrong or Keycloak rejects the request.
    /// </summary>
    Task ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
}
