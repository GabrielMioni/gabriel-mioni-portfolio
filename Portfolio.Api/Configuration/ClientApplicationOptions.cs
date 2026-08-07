namespace Portfolio.Api.Configuration;

public sealed class ClientApplicationOptions
{
    public const string SectionName = "ClientApplications";

    public string AdminOrigin { get; init; } = string.Empty;

    public string PublicOrigin { get; init; } = string.Empty;

    public bool IsConfigured =>
        IsHttpOrigin(AdminOrigin)
        && IsHttpOrigin(PublicOrigin);

    private static bool IsHttpOrigin(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && string.IsNullOrEmpty(uri.UserInfo)
            && uri.AbsolutePath == "/"
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment);
    }
}
