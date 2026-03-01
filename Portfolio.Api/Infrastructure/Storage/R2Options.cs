namespace Portfolio.Api.Infrastructure.Storage;
public class R2Options
{
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string Endpoint { get; set; } = default!;
    public string Bucket { get; set; } = default!;
    public string PublicBaseUrl { get; set; } = default!;
}