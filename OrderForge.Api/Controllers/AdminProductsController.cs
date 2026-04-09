using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Admin;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/products")]
public sealed class AdminProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.SupplierStaff)]
    [ProducesResponseType(typeof(AdminProductsListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminProductsListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetAdminProductsQuery(page, pageSize, search), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.SupplierAdmin)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(List), new { }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{productId:int}")]
    [Authorize(Policy = AuthorizationPolicies.SupplierAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int productId, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new DeleteProductCommand(productId), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
