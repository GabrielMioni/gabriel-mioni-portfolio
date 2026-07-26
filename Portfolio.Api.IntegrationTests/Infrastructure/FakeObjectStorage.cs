using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.IntegrationTests.Infrastructure;

internal sealed class FakeObjectStorage : IObjectStorage
{
    private readonly List<string> _deletedKeys = [];

    public IReadOnlyList<string> DeletedKeys => _deletedKeys;

    public string CreatePresignedPutUrl(
        string key,
        string contentType,
        TimeSpan expiresIn)
        => $"https://storage.test/upload/{key}";

    public string GetPublicUrl(string key) => $"https://storage.test/{key}";

    public Task<bool> ObjectExistsAsync(string key, CancellationToken ct)
        => Task.FromResult(true);

    public Task DeleteImagesAsync(IEnumerable<string> keys, CancellationToken ct)
    {
        _deletedKeys.AddRange(keys);

        return Task.CompletedTask;
    }
}
