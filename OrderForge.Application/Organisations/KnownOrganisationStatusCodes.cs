namespace OrderForge.Application.Organisations;

/// <summary>Codes stored in <c>organisation_statuses.code</c>.</summary>
public static class KnownOrganisationStatusCodes
{
    public const string Active = "Active";

    public const string Inactive = "Inactive";

    public const string Unknown = "Unknown";

    public static readonly IReadOnlyCollection<string> All = [Active, Inactive, Unknown];
}
