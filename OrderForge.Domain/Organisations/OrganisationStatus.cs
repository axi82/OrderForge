namespace OrderForge.Domain.Organisations;

/// <summary>
/// Lookup row in <c>organisation_statuses</c>. Seeded ids are stable for migrations and tests.
/// </summary>
public class OrganisationStatus
{
    public const int ActiveId = 1;

    public const int InactiveId = 2;

    public const int UnknownId = 3;

    public int Id { get; set; }

    /// <summary>Display code, e.g. Active, Inactive, Unknown.</summary>
    public string Code { get; set; } = string.Empty;
}
