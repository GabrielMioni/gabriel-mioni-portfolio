using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Types;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects;

[ExtendObjectType(typeof(ProjectMutation))]
public class ProjectImageMutation
{
    public async Task<RequestProjectImageUploadsPayload> PrepareProjectImageUploads(
        RequestProjectImageUploadsInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var instructions = await images.PrepareImageUploadAsync(input, ct);

        return new RequestProjectImageUploadsPayload(input.ProjectId, instructions);
    }
}