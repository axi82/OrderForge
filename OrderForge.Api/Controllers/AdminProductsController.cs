using System.Text.Json;
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
    private static readonly JsonSerializerOptions ProductJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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

    /// <summary>Create a product with optional images (multipart: form field <c>product</c> = JSON for <see cref="CreateProductCommand"/>, files named <c>images</c>, <c>mainImageIndex</c> when images are present).</summary>
    [HttpPost("with-images")]
    [Authorize(Policy = AuthorizationPolicies.SupplierAdmin)]
    [RequestSizeLimit(100 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateWithImages(
        [FromForm] string product,
        [FromForm] int? mainImageIndex,
        CancellationToken cancellationToken)
    {
        CreateProductCommand? productCmd;
        try
        {
            productCmd = JsonSerializer.Deserialize<CreateProductCommand>(product ?? "{}", ProductJsonOptions);
        }
        catch (JsonException ex)
        {
            return BadRequest(new { message = "Invalid product JSON.", detail = ex.Message });
        }

        if (productCmd is null)
        {
            return BadRequest(new { message = "Product payload is required." });
        }

        var uploads = new List<CreateProductImageUpload>();
        var files = Request.Form.Files.GetFiles("images");
        try
        {
            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                await using (var readStream = file.OpenReadStream())
                {
                    var ms = new MemoryStream();
                    await readStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                    ms.Position = 0;
                    if (ms.Length > CreateProductWithImagesCommandValidator.MaxImageBytes)
                    {
                        await ms.DisposeAsync().ConfigureAwait(false);
                        return BadRequest(
                            new
                            {
                                message =
                                    $"Each image must be at most {CreateProductWithImagesCommandValidator.MaxImageBytes} bytes."
                            });
                    }

                    uploads.Add(
                        new CreateProductImageUpload
                        {
                            Content = ms,
                            FileName = string.IsNullOrWhiteSpace(file.FileName) ? "image" : file.FileName,
                            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? null : file.ContentType
                        });
                }
            }

            var command = new CreateProductWithImagesCommand(productCmd, uploads, mainImageIndex);
            var created = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(List), new { }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        finally
        {
            foreach (var u in uploads)
            {
                await u.Content.DisposeAsync().ConfigureAwait(false);
            }
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
