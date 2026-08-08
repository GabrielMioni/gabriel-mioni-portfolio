using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Extensions.Options;
using Portfolio.Api.Infrastructure.Storage;
using System.Net;

namespace Portfolio.Api.Services.Storage;
public sealed class ObjectStorage : IObjectStorage
{
    private const int MaxDeleteKeysPerRequest = 1_000;

    private readonly IAmazonS3 _s3;
    private readonly R2Options _opts;

    public ObjectStorage(IAmazonS3 s3, IOptions<R2Options> opts)
    {
        _s3 = s3;
        _opts = opts.Value;
    }

    public string CreatePresignedPutUrl(
        string key,
        string contentType,
        long contentLength,
        TimeSpan expiresIn)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _opts.Bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiresIn),
            ContentType = contentType
        };

        req.Headers.ContentLength = contentLength;

        return _s3.GetPreSignedURL(req);
    }

    public async Task DeleteImagesAsync(IEnumerable<string> keys, CancellationToken ct = default)
    {
        var keyList = keys
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToList();

        if (keyList.Count == 0)
        {
            return;
        }

        foreach (var keyBatch in keyList.Chunk(MaxDeleteKeysPerRequest))
        {
            var req = new DeleteObjectsRequest
            {
                BucketName = _opts.Bucket,
                Objects = keyBatch
                    .Select(k => new KeyVersion { Key = k })
                    .ToList()
            };

            await _s3.DeleteObjectsAsync(req, ct);
        }
    }

    public async Task<bool> ObjectExistsAsync(
        string key,
        CancellationToken ct = default)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = _opts.Bucket,
            Key = key
        };

        try
        {
            await _s3.GetObjectMetadataAsync(request, ct);
            return true;
        }
        catch (AmazonS3Exception exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public string GetPublicUrl(string key)
        => $"{_opts.PublicBaseUrl.TrimEnd('/')}/{key}";
}
