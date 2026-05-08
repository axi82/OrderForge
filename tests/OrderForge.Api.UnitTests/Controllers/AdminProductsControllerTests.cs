using System.Text;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderForge.Api.Controllers;
using OrderForge.Application.Admin;

namespace OrderForge.Api.UnitTests.Controllers;

public sealed class AdminProductsControllerTests
{
    private readonly Mock<ISender> _mediator = new();

    private static string MinimalValidProductJson() =>
        """
        {
          "sku": "SKU-1",
          "productCode": "PC-1",
          "name": "Widget",
          "shortDescription": "A widget",
          "quantityInStock": 0,
          "quantityAllocated": 0,
          "quantityOnOrder": 0,
          "freeStock": 0,
          "costPrice": 0,
          "basePrice": 0,
          "isActive": true
        }
        """;

    private static DefaultHttpContext CreateHttpContextWithFormFiles(params IFormFile[] files)
    {
        var httpContext = new DefaultHttpContext();
        var fileCollection = new FormFileCollection();
        foreach (var f in files)
        {
            fileCollection.Add(f);
        }

        var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), fileCollection);
        httpContext.Features.Set<IFormFeature>(new FormFeature(form));
        return httpContext;
    }

    private static ProductDto SampleProductDto() =>
        new(
            Id: 42,
            Sku: "SKU-1",
            ProductCode: "PC-1",
            Name: "Widget",
            ShortDescription: "A widget",
            Description: null,
            Brand: null,
            CommodityCodeDescription: null,
            SupplierAccountCode: null,
            PartNumber: null,
            QuantityInStock: 0,
            QuantityAllocated: 0,
            QuantityOnOrder: 0,
            FreeStock: 0,
            Barcode: null,
            CostPrice: 0,
            BasePrice: 0,
            IsActive: true,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            CreatedBy: "tester",
            Images: [new ProductImageDto(1, "https://images.orderforge.co.uk/products/42/x.jpg", 0, true)]);

    [Fact]
    public async Task CreateWithImages_invalid_product_json_returns_BadRequest()
    {
        var sut = new AdminProductsController(_mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContextWithFormFiles() }
        };

        var result = await sut.CreateWithImages("{ not json", mainImageIndex: null, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().NotBeNull();
        _mediator.Verify(
            m => m.Send(It.IsAny<CreateProductWithImagesCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateWithImages_null_product_deserialization_returns_BadRequest()
    {
        var sut = new AdminProductsController(_mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContextWithFormFiles() }
        };

        var result = await sut.CreateWithImages("null", mainImageIndex: null, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _mediator.Verify(
            m => m.Send(It.IsAny<CreateProductWithImagesCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateWithImages_image_over_max_bytes_returns_BadRequest_without_calling_mediator()
    {
        var oversized = CreateProductWithImagesCommandValidator.MaxImageBytes + 1;
        var bytes = new byte[oversized];
        await using var backing = new MemoryStream(bytes);
        var file = new FormFile(backing, 0, oversized, "images", "big.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var sut = new AdminProductsController(_mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContextWithFormFiles(file) }
        };

        var result = await sut.CreateWithImages(MinimalValidProductJson(), mainImageIndex: 0, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _mediator.Verify(
            m => m.Send(It.IsAny<CreateProductWithImagesCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateWithImages_mediator_InvalidOperation_returns_BadRequest()
    {
        await using var imageBytes = new MemoryStream(Encoding.UTF8.GetBytes("img"));
        var file = new FormFile(imageBytes, 0, imageBytes.Length, "images", "a.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var sut = new AdminProductsController(_mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContextWithFormFiles(file) }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<CreateProductWithImagesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("duplicate sku"));

        var result = await sut.CreateWithImages(MinimalValidProductJson(), mainImageIndex: 0, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateWithImages_valid_multipart_sends_CreateProductWithImagesCommand_and_returns_Created()
    {
        await using var imageBytes = new MemoryStream(Encoding.UTF8.GetBytes("abc"));
        var file = new FormFile(imageBytes, 0, imageBytes.Length, "images", "photo.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var sut = new AdminProductsController(_mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContextWithFormFiles(file) }
        };

        var returned = SampleProductDto();
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateProductWithImagesCommand>(), It.IsAny<CancellationToken>()))
            .Returns<CreateProductWithImagesCommand, CancellationToken>((cmd, _) =>
            {
                cmd.Images.Should().ContainSingle();
                cmd.Images[0].Content.Length.Should().Be(3);
                cmd.Images[0].Content.CanSeek.Should().BeTrue();
                return Task.FromResult(returned);
            });

        var result = await sut.CreateWithImages(MinimalValidProductJson(), mainImageIndex: 0, CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().Be(returned);

        _mediator.Verify(
            m => m.Send(
                    It.Is<CreateProductWithImagesCommand>(c =>
                        c.Images.Count == 1
                        && c.MainImageIndex == 0
                        && c.Product.Sku == "SKU-1"
                        && c.Product.ProductCode == "PC-1"
                        && c.Images[0].FileName == "photo.jpg"
                        && c.Images[0].ContentType == "image/jpeg"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task CreateWithImages_skips_zero_length_files_and_sends_empty_images_when_all_empty()
    {
        var empty = new FormFile(new MemoryStream(), 0, 0, "images", "skip.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var sut = new AdminProductsController(_mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContextWithFormFiles(empty) }
        };

        var returned = SampleProductDto();
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateProductWithImagesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(returned);

        var result = await sut.CreateWithImages(MinimalValidProductJson(), mainImageIndex: null, CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        _mediator.Verify(
            m => m.Send(
                    It.Is<CreateProductWithImagesCommand>(c => c.Images.Count == 0 && c.MainImageIndex == null),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }
}
