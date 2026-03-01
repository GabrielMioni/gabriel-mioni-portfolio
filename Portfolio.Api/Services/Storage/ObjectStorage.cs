using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Extensions.Options;
using Portfolio.Api.Infrastructure.Storage;

namespace Portfolio.Api.Services.Storage;
public sealed class ObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 _s3;
    private readonly R2Options _opts;

    public ObjectStorage(IAmazonS3 s3, IOptions<R2Options> opts)
    {
        _s3 = s3;
        _opts = opts.Value;
    }

    public string CreatePresignedPutUrl(string key, string contentType, TimeSpan expiresIn)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _opts.Bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiresIn),
            ContentType = contentType
        };

        return _s3.GetPreSignedURL(req);
    }

    public string GetPublicUrl(string key)
        => $"{_opts.PublicBaseUrl.TrimEnd('/')}/{key}";
}