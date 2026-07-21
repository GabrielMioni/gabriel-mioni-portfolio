using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.IntegrationTests.Infrastructure;

internal sealed class FakeObjectStorage : IObjectStorage
{
    public string CreatePresignedPutUrl(
        string key,
        string contentType,
        TimeSpan expiresIn)
        => $"https://storage.test/upload/{key}";

    public string GetPublicUrl(string key) => $"https://storage.test/{key}";

    public Task<bool> ObjectExistsAsync(string key, CancellationToken ct)
        => Task.FromResult(true);

    public Task DeleteImagesAsync(IEnumerable<string> keys, CancellationToken ct)
        => Task.CompletedTask;
}
