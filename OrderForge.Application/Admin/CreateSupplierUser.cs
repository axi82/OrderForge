using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderForge.Application.Common;
using OrderForge.Application.Common.Services;

namespace OrderForge.Application.Admin;

public sealed record CreateSupplierUserCommand(
    string Email,
    string? FirstName,
    string? LastName,
    string RealmRoleName,
    string? TemporaryPassword) : IRequest<CreateSupplierUserResult>;

public sealed record CreateSupplierUserResult(string UserId, string Email, string Username);

public sealed class CreateSupplierUserCommandValidator : AbstractValidator<CreateSupplierUserCommand>
{
    public CreateSupplierUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.RealmRoleName)
            .NotEmpty()
            .Must(n => KnownRealmRoles.SupplierCreateRoles.Contains(n, StringComparer.Ordinal))
            .WithMessage($"Role must be one of: {string.Join(", ", KnownRealmRoles.SupplierCreateRoles)}.");
        RuleFor(x => x.TemporaryPassword).MinimumLength(8).When(x => x.TemporaryPassword is not null);
    }
}

public sealed class CreateSupplierUserCommandHandler(
    IKeycloakAdminService keycloakAdmin,
    ICurrentUser currentUser,
    ILogger<CreateSupplierUserCommandHandler> logger)
    : IRequestHandler<CreateSupplierUserCommand, CreateSupplierUserResult>
{
    public async Task<CreateSupplierUserResult> Handle(
        CreateSupplierUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsSupplierAdmin)
        {
            throw new UnauthorizedAccessException("Only a SupplierAdmin can create supplier users.");
        }

        var username = UsernameFromEmail(request.Email);
        var create = new CreateKeycloakUserRequest(
            username,
            request.Email,
            request.FirstName,
            request.LastName,
            Enabled: true,
            request.TemporaryPassword);

        var created = await keycloakAdmin.CreateUserAsync(create, cancellationToken);
        await keycloakAdmin.AssignRealmRoleAsync(created.UserId, request.RealmRoleName, cancellationToken);

        logger.LogInformation(
            "Created supplier user {Email} with role {Role} by {Actor}",
            request.Email,
            request.RealmRoleName,
            currentUser.UserId ?? "(unknown)");

        return new CreateSupplierUserResult(created.UserId, request.Email, created.Username);
    }

    private static string UsernameFromEmail(string email)
    {
        var at = email.IndexOf('@', StringComparison.Ordinal);
        var local = at > 0 ? email[..at] : email;
        var sanitized = new string(local.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        return string.IsNullOrEmpty(sanitized) ? "user" : sanitized.ToLowerInvariant();
    }
}
