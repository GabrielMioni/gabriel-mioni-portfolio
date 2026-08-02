namespace Portfolio.Api.Authentication;

public sealed class GitHubAuthenticationOptions
{
    public const string SectionName = "Authentication:GitHub";

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string AllowedUserId { get; init; } = string.Empty;

    public string AdminBaseUrl { get; init; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret)
        && !string.IsNullOrWhiteSpace(AllowedUserId)
        && Uri.TryCreate(AdminBaseUrl, UriKind.Absolute, out var adminUri)
        && (adminUri.Scheme == Uri.UriSchemeHttp || adminUri.Scheme == Uri.UriSchemeHttps);
}
