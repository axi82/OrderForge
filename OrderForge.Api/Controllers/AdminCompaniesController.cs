using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Admin;
using OrderForge.Application.Organisations;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/companies")]
public sealed class AdminCompaniesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.SupplierStaff)]
    [ProducesResponseType(typeof(IReadOnlyList<OrganisationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrganisationDto>>> List(CancellationToken cancellationToken)
    {
        var list = await sender.Send(new GetAdminCompaniesQuery(), cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.SupplierAdmin)]
    [ProducesResponseType(typeof(OrganisationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganisationDto>> Create(
        [FromBody] CreateCustomerCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var created = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { }, created);
    }
}
