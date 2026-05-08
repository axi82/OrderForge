namespace OrderForge.Infrastructure.Storage;

public sealed class BunnyStorageOptions
{
    public const string SectionName = "BunnyStorage";

    public string StorageZoneName { get; set; } = string.Empty;

    public string ApiAccessKey { get; set; } = string.Empty;

    public string Region { get; set; } = "de";

    public string PublicBaseUrl { get; set; } = "https://images.orderforge.co.uk";
}
