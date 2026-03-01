using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Types;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.GraphQL.Projects;

[ExtendObjectType(typeof(ProjectMutation))]
public class ProjectImageMutation
{
    public RequestUploadPayload RequestTestUpload(RequestUploadInput input, [Service] IR2Storage storage)
    {
        var ext = input.ContentType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            _ => "bin"
        };

        var key = $"test/{Guid.NewGuid():N}.{ext}";

        var uploadUrl = storage.CreatePresignedPutUrl(
            key,
            input.ContentType,
            TimeSpan.FromMinutes(5));

        var publicUrl = storage.GetPublicUrl(key);

        return new RequestUploadPayload(key, uploadUrl, publicUrl);
    }
}