using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrderForge.Application.Admin;
using OrderForge.Application.Common;

namespace OrderForge.Api.Security;

public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    public string? PreferredUsername =>
        User?.FindFirstValue("preferred_username")
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public bool IsSupplierAdmin => User?.IsInRole(KnownRealmRoles.SupplierAdmin) == true;

    public bool IsSupplierViewer => User?.IsInRole(KnownRealmRoles.SupplierViewer) == true;

    public bool IsCompanyAdmin => User?.IsInRole(KnownRealmRoles.CompanyAdmin) == true;

    public bool IsCustomer => User?.IsInRole(KnownRealmRoles.Customer) == true;

    public string? KeycloakOrganizationId => User?.FindFirstValue(KeycloakJwtClaimsMapper.KeycloakOrganizationIdClaim);
}
