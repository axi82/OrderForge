namespace OrderForge.Application.Common;

/// <summary>
/// Keycloak Admin REST returned an error or an unexpected response.
/// </summary>
public sealed class KeycloakAdminException : Exception
{
    public KeycloakAdminException(string message)
        : base(message)
    {
    }

    public KeycloakAdminException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public int? StatusCode { get; init; }

    public string? ResponseBody { get; init; }
}
