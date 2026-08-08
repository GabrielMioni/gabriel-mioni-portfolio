namespace Portfolio.Api.Infrastructure.Storage;
public class R2Options
{
    public const string SectionName = "R2";

    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string Endpoint { get; set; } = default!;
    public string Bucket { get; set; } = default!;
    public string PublicBaseUrl { get; set; } = default!;

    public static bool IsHttpUrl(string? value, bool requireHttps)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps
                || (!requireHttps && uri.Scheme == Uri.UriSchemeHttp))
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment);
    }
}
