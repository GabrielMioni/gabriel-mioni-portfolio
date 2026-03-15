using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Payloads;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects;

[ExtendObjectType(typeof(ProjectMutation))]
public class ProjectImageMutation
{
    public async Task<RequestProjectImageUploadsPayload> PrepareProjectImageUploads(
        PrepareProjectImageUploadsInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var instructions = await images.PrepareImageUploadAsync(input, ct);

        return new RequestProjectImageUploadsPayload(input.ProjectId, instructions);
    }

    public async Task<FinalizeProjectImageUploadsPayload> FinalizeProjectImageUploads(
        FinalizeProjectImageUploadsInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var project = await images.FinalizeImageUploadAsync(input, ct);

        return new FinalizeProjectImageUploadsPayload(project);
    }
}