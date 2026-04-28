using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Profile;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(ISender sender) : ControllerBase
{
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateMyProfileBody body,
        CancellationToken cancellationToken)
    {
        await sender.Send(
                new UpdateMyProfileCommand(body.FirstName ?? "", body.LastName ?? ""),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeMyPasswordBody body,
        CancellationToken cancellationToken)
    {
        await sender.Send(
                new ChangeMyPasswordCommand(
                    body.CurrentPassword ?? "",
                    body.NewPassword ?? "",
                    body.ConfirmPassword ?? ""),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }
}

public sealed record UpdateMyProfileBody(string? FirstName, string? LastName);

public sealed record ChangeMyPasswordBody(string? CurrentPassword, string? NewPassword, string? ConfirmPassword);
