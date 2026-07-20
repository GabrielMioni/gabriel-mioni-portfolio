namespace Portfolio.Api.Services.Storage;
public interface IObjectStorage
{
    string CreatePresignedPutUrl(string key, string contentType, TimeSpan expiresIn);
    string GetPublicUrl(string key);
    Task<bool> ObjectExistsAsync(string key, CancellationToken ct);
    Task DeleteImagesAsync(IEnumerable<string> keys, CancellationToken ct);
}
