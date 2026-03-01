namespace Portfolio.Api.Services.Storage;
public interface IR2Storage
{
    string CreatePresignedPutUrl(string key, string contentType, TimeSpan expiresIn);
    string GetPublicUrl(string key);
}