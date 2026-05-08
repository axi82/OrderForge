namespace OrderForge.Application.Admin;

public sealed record ProductImageDto(int Id, string Url, int SortOrder, bool IsMain);
