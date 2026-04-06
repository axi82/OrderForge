using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Admin;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/users")]
public sealed class AdminUsersController(ISender sender) : ControllerBase
{
    [HttpPost("invite")]
    [Authorize(Policy = AuthorizationPolicies.InviteUsers)]
    [ProducesResponseType(typeof(InviteUserToCompanyResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InviteUserToCompanyResult>> Invite(
        [FromBody] InviteUserToCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("supplier")]
    [Authorize(Policy = AuthorizationPolicies.SupplierAdmin)]
    [ProducesResponseType(typeof(CreateSupplierUserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateSupplierUserResult>> CreateSupplier(
        [FromBody] CreateSupplierUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
