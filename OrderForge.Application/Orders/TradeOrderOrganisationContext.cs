using OrderForge.Application.Common;
using OrderForge.Application.Organisations;

namespace OrderForge.Application.Orders;

internal static class TradeOrderOrganisationContext
{
    public static async Task<int?> ResolveTradeOrganisationIdAsync(
        ICurrentUser currentUser,
        IOrganisationRepository organisations,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsSupplierAdmin || currentUser.IsSupplierViewer)
        {
            return null;
        }

        if (!(currentUser.IsCustomer || currentUser.IsCompanyAdmin))
        {
            return null;
        }

        if (string.IsNullOrEmpty(currentUser.KeycloakOrganizationId))
        {
            return null;
        }

        var org = await organisations
            .GetByKeycloakOrganizationIdAsync(currentUser.KeycloakOrganizationId, cancellationToken)
            .ConfigureAwait(false);

        return org?.Id;
    }
}
