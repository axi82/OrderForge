using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Catalog;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/catalog/products")]
public sealed class CatalogProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CatalogProductsListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogProductsListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCatalogProductsQuery(page, pageSize, search), cancellationToken);
        return Ok(result);
    }
}
