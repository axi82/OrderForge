using OrderForge.Application.Common;
using OrderForge.Domain.Organisations;

namespace OrderForge.Application.Organisations;

internal static class OrganisationVisibility
{
    public static bool CanView(ICurrentUser user, Organisation organisation)
    {
        if (user.IsSupplierAdmin || user.IsSupplierViewer)
        {
            return true;
        }

        if (user.IsCompanyAdmin
            && !string.IsNullOrEmpty(user.KeycloakOrganizationId)
            && string.Equals(
                organisation.KeycloakOrganizationId,
                user.KeycloakOrganizationId,
                StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
