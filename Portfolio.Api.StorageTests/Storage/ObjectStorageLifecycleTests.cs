using System.Net.Http.Headers;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;
using Portfolio.Api.Services.Storage;
using Portfolio.Api.StorageTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.StorageTests.Storage;

public sealed class ObjectStorageLifecycleTests
{
    [RequiresR2Fact]
    public async Task ObjectStorage_WhenObjectUploaded_CanFindAndDeleteIt()
    {
        var options = R2StorageTestEnvironment.LoadOptions();
        var config = new AmazonS3Config
        {
            ServiceURL = options.Endpoint,
            ForcePathStyle = true
        };

        using var s3 = new AmazonS3Client(
            new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            config);
        var storage = new ObjectStorage(s3, Options.Create(options));
        using var client = new HttpClient();
        var key = $"integration-tests/{Guid.NewGuid():N}/probe.bin";
        byte[] payload = [0x43, 0x6F, 0x64, 0x65, 0x78];

        try
        {
            var uploadUrl = storage.CreatePresignedPutUrl(
                key,
                "application/octet-stream",
                payload.Length,
                TimeSpan.FromMinutes(5));
            using var content = new ByteArrayContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue(
                "application/octet-stream");

            using var response = await client.PutAsync(uploadUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.IsSuccessStatusCode,
                $"R2 upload failed with {(int)response.StatusCode} " +
                $"{response.StatusCode}: {responseBody}");
            Assert.True(await storage.ObjectExistsAsync(key, CancellationToken.None));

            await storage.DeleteImagesAsync([key], CancellationToken.None);

            Assert.False(await storage.ObjectExistsAsync(key, CancellationToken.None));
        }
        finally
        {
            await storage.DeleteImagesAsync([key], CancellationToken.None);
        }
    }
}
