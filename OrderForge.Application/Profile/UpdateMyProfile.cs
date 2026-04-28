using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Common.Services;

namespace OrderForge.Application.Profile;

public sealed record UpdateMyProfileCommand(string FirstName, string LastName) : IRequest;

public sealed class UpdateMyProfileCommandHandler(
    ICurrentUser currentUser,
    IKeycloakAdminService keycloakAdmin)
    : IRequestHandler<UpdateMyProfileCommand>
{
    public async Task Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            throw new InvalidOperationException("Your account id is missing from the token. Try signing in again.");
        }

        var first = request.FirstName.Trim();
        var last = request.LastName.Trim();
        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(last))
        {
            throw new InvalidOperationException("First name and last name are required.");
        }

        await keycloakAdmin
            .UpdateRealmUserNamesAsync(currentUser.UserId, first, last, cancellationToken)
            .ConfigureAwait(false);
    }
}
