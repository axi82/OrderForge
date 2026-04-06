using OrderForge.Application.Common;
using OrderForge.Domain.Organisations;

namespace OrderForge.Application.Organisations;

public interface IOrganisationRepository : IRepository<Organisation>
{
    Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Organisation?> GetByKeycloakOrganizationIdAsync(
        string keycloakOrganizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Organisation>> GetAllWithKeycloakOrgAsync(CancellationToken cancellationToken = default);
}
