using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Types;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.Services;

public class ProjectImageService
{
    private readonly IObjectStorage _storage;

    public ProjectImageService(IObjectStorage storage)
    {
        _storage = storage;
    }
    static string ExtFor(string contentType) => contentType switch
    {
        "image/jpeg" => "jpg",
        "image/png" => "png",
        "image/webp" => "webp",
        _ => "bin"
    };

    public RequestUploadPayload GetPresignedUrl(RequestUploadInput input)
    {
        var ext = ExtFor(input.ContentType);

        var key = $"test/{Guid.NewGuid():N}.{ext}";

        var uploadUrl = _storage.CreatePresignedPutUrl(
            key,
            input.ContentType,
            TimeSpan.FromMinutes(5));

        var publicUrl = _storage.GetPublicUrl(key);

        return new RequestUploadPayload(key, uploadUrl, publicUrl);
    }
}
