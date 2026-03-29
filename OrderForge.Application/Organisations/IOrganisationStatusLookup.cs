namespace OrderForge.Application.Organisations;

public interface IOrganisationStatusLookup
{
    /// <summary>Returns the status id for an exact <paramref name="code"/>, or null if not found.</summary>
    Task<int?> GetIdForCodeAsync(string code, CancellationToken cancellationToken = default);
}
