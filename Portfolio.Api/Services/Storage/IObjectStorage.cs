namespace Portfolio.Api.Services.Storage;
public interface IObjectStorage
{
    string CreatePresignedPutUrl(string key, string contentType, TimeSpan expiresIn);
    string GetPublicUrl(string key);
}