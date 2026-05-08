using BunnyCDN.Net.Storage;
using Microsoft.Extensions.Options;
using OrderForge.Application.Storage;

namespace OrderForge.Infrastructure.Storage;

public sealed class BunnyObjectStorage(IOptions<BunnyStorageOptions> options) : IBunnyObjectStorage
{
    private readonly BunnyStorageOptions _options = options.Value;
    private readonly BunnyCDNStorage _client = new(
        options.Value.StorageZoneName,
        options.Value.ApiAccessKey,
        string.IsNullOrWhiteSpace(options.Value.Region) ? "de" : options.Value.Region);

    public string GetPublicUrl(string storageRelativePath)
    {
        var path = NormalizeRelativePath(storageRelativePath);
        var baseUrl = _options.PublicBaseUrl.Trim().TrimEnd('/');
        return $"{baseUrl}/{path}";
    }

    public Task UploadAsync(
        Stream content,
        string storageRelativePath,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        _ = contentType;
        cancellationToken.ThrowIfCancellationRequested();
        var relative = NormalizeRelativePath(storageRelativePath);
        var bunnyPath = $"{_options.StorageZoneName}/{relative}";
        return _client.UploadAsync(content, bunnyPath);
    }

    public Task DeleteAsync(string storageRelativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var relative = NormalizeRelativePath(storageRelativePath);
        var bunnyPath = $"{_options.StorageZoneName}/{relative}";
        return _client.DeleteObjectAsync(bunnyPath);
    }

    private static string NormalizeRelativePath(string storageRelativePath) =>
        storageRelativePath.Trim().Replace('\\', '/').TrimStart('/');
}
