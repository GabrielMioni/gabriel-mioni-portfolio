using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Types;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects;

[ExtendObjectType(typeof(ProjectMutation))]
public class ProjectImageMutation
{
    //public RequestUploadPayload RequestTestUpload(RequestUploadInput input, [Service] ProjectImageService service)
    //{
    //    return service.GetPresignedUrl(input);
    //}

    public async Task<RequestProjectImageUploadsPayload> RequestProjectImageUploads(
        RequestProjectImageUploadsInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var instructions = await images.RequestUploadsAsync(
            input.ProjectId,
            input.Items,
            ct);

        return new RequestProjectImageUploadsPayload(instructions);
    }
}