using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Common.Services;

namespace OrderForge.Application.Profile;

public sealed record ChangeMyPasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest;

public sealed class ChangeMyPasswordCommandHandler(
    ICurrentUser currentUser,
    IKeycloakUserPasswordValidator passwordValidator,
    IKeycloakAdminService keycloakAdmin)
    : IRequestHandler<ChangeMyPasswordCommand>
{
    public const int MinimumPasswordLength = 8;

    public async Task Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            throw new InvalidOperationException("Your account id is missing from the token. Try signing in again.");
        }

        if (string.IsNullOrWhiteSpace(currentUser.PreferredUsername))
        {
            throw new InvalidOperationException(
                "Cannot verify your password because your login name is missing from the session. Try signing in again.");
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            throw new InvalidOperationException("Current password is required.");
        }

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("New password and confirmation do not match.");
        }

        if (request.NewPassword.Length < MinimumPasswordLength)
        {
            throw new InvalidOperationException($"Password must be at least {MinimumPasswordLength} characters.");
        }

        if (string.Equals(request.CurrentPassword, request.NewPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("New password must be different from your current password.");
        }

        await passwordValidator
            .ValidateCredentialsAsync(currentUser.PreferredUsername, request.CurrentPassword, cancellationToken)
            .ConfigureAwait(false);

        await keycloakAdmin
            .SetRealmUserPasswordAsync(currentUser.UserId, request.NewPassword, temporary: false, cancellationToken)
            .ConfigureAwait(false);
    }
}
