namespace OrderForge.Application.Storage;

public interface IBunnyObjectStorage
{
    string GetPublicUrl(string storageRelativePath);

    Task UploadAsync(
        Stream content,
        string storageRelativePath,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageRelativePath, CancellationToken cancellationToken = default);
}
