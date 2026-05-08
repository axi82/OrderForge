namespace OrderForge.Application.Admin;

/// <summary>One image stream for <see cref="CreateProductWithImagesCommand"/>. The API layer owns streams and disposes them after <c>Send</c> completes.</summary>
public sealed class CreateProductImageUpload
{
    public required Stream Content { get; init; }

    public required string FileName { get; init; }

    public string? ContentType { get; init; }
}
