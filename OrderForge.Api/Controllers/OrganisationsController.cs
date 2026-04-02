using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Organisations;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class OrganisationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrganisationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrganisationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await sender.Send(new GetOrganisationsQuery(), cancellationToken);
        return Ok(list);
    }

    [HttpGet("{organisationId:int}")]
    [ProducesResponseType(typeof(OrganisationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationDto>> GetById(int organisationId, CancellationToken cancellationToken)
    {
        var dto = await sender.Send(new GetOrganisationByIdQuery(organisationId), cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrganisationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganisationDto>> Create(
        [FromBody] CreateOrganisationCommand command,
        CancellationToken cancellationToken)
    {
        var created = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { organisationId = created.Id }, created);
    }

    [HttpPut("{organisationId:int}")]
    [ProducesResponseType(typeof(OrganisationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationDto>> Update(
        int organisationId,
        [FromBody] UpdateOrganisationRequest body,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateOrganisationCommand(
                organisationId,
                body.Name,
                body.TradingAs,
                body.CompanyNumber,
                body.VatNumber,
                body.AccountNumber,
                body.Status),
            cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{organisationId:int}/status")]
    [ProducesResponseType(typeof(OrganisationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationDto>> ChangeStatus(
        int organisationId,
        [FromBody] ChangeOrganisationStatusRequest body,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new ChangeOrganisationStatusCommand(organisationId, body.Status),
            cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{organisationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int organisationId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteOrganisationCommand(organisationId), cancellationToken);
        return NoContent();
    }
}

public sealed class ChangeOrganisationStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class UpdateOrganisationRequest
{
    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? AccountNumber { get; set; }

    public string Status { get; set; } = string.Empty;
}
