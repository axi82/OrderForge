using System.Net.Http.Json;
using System.Text.Json;

namespace OrderForge.Client.Services;

public interface IProfileApiClient
{
    Task UpdateProfileAsync(string firstName, string lastName, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default);
}

public sealed class ProfileApiClient(HttpClient http) : IProfileApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task UpdateProfileAsync(string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var response = await http
            .PutAsJsonAsync(
                "api/profile",
                new UpdateMyProfileRequest(firstName, lastName),
                JsonOptions,
                cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        var response = await http
            .PostAsJsonAsync(
                "api/profile/password",
                new ChangeMyPasswordRequest(currentPassword, newPassword, confirmPassword),
                JsonOptions,
                cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new HttpRequestException(
            $"Profile API error {(int)response.StatusCode}: {text}",
            null,
            response.StatusCode);
    }

    private sealed record UpdateMyProfileRequest(string FirstName, string LastName);

    private sealed record ChangeMyPasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
