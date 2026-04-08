using FluentValidation;
using MediatR;
using OrderForge.Application.Common.Services;

namespace OrderForge.Application.Admin;

public sealed record AdminUserRowDto(
    string Id,
    string? FirstName,
    string? LastName,
    string? Email,
    bool Enabled,
    string OrganisationNames,
    DateTime? LastLoginUtc);

public sealed record AdminUsersListResponse(
    IReadOnlyList<AdminUserRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search);

public sealed record GetAdminUsersQuery(int Page, int PageSize, string? Search) : IRequest<AdminUsersListResponse>;

public sealed class GetAdminUsersQueryValidator : AbstractValidator<GetAdminUsersQuery>
{
    public GetAdminUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200).When(x => x.Search is not null);
    }
}

public sealed class GetAdminUsersQueryHandler(IKeycloakAdminService keycloak)
    : IRequestHandler<GetAdminUsersQuery, AdminUsersListResponse>
{
    private const int KeycloakBatchSize = 50;

    public async Task<AdminUsersListResponse> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var pageStart = (request.Page - 1) * request.PageSize;
        var pageEnd = pageStart + request.PageSize;

        var pageUsers = new List<KeycloakRealmUserBrief>();
        var humanOrdinal = 0;
        var kcFirst = 0;

        while (true)
        {
            var batch = await keycloak
                .SearchRealmUsersAsync(kcFirst, KeycloakBatchSize, request.Search, cancellationToken)
                .ConfigureAwait(false);
            if (batch.Count == 0)
            {
                break;
            }

            foreach (var u in batch)
            {
                if (IsServiceAccount(u))
                {
                    continue;
                }

                if (humanOrdinal >= pageStart && humanOrdinal < pageEnd)
                {
                    pageUsers.Add(u);
                }

                humanOrdinal++;
            }

            kcFirst += batch.Count;
            if (batch.Count < KeycloakBatchSize)
            {
                break;
            }
        }

        var totalHumans = humanOrdinal;

        var rowTasks = pageUsers.Select(u => MapRowAsync(u, cancellationToken));
        var rows = await Task.WhenAll(rowTasks).ConfigureAwait(false);

        return new AdminUsersListResponse(
            rows,
            request.Page,
            request.PageSize,
            totalHumans,
            request.Search);
    }

    private static bool IsServiceAccount(KeycloakRealmUserBrief u) =>
        !string.IsNullOrEmpty(u.ServiceAccountClientId)
        || (u.Username?.StartsWith("service-account-", StringComparison.OrdinalIgnoreCase) ?? false);

    private async Task<AdminUserRowDto> MapRowAsync(KeycloakRealmUserBrief u, CancellationToken cancellationToken)
    {
        var orgTask = keycloak.GetOrganizationNamesForUserAsync(u.Id, cancellationToken);
        var sessionTask = keycloak.GetLatestSessionLastAccessUtcAsync(u.Id, cancellationToken);
        await Task.WhenAll(orgTask, sessionTask).ConfigureAwait(false);

        var names = await orgTask.ConfigureAwait(false);
        var orgLabel = names.Count == 0 ? string.Empty : string.Join(", ", names);
        var lastLogin = await sessionTask.ConfigureAwait(false);

        return new AdminUserRowDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.Enabled,
            orgLabel,
            lastLogin);
    }
}
