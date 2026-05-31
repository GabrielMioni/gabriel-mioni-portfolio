using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Admin;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AdminProjectImageMutation
{
    public async Task<PrepareProjectImageUploadsPayload> PrepareProjectImageUploads(
        PrepareProjectImageUploadsInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var instructions = await images.PrepareImageUploadAsync(input, ct);

        return new PrepareProjectImageUploadsPayload(input.ProjectId, instructions);
    }

    public async Task<FinalizeProjectImageUploadsPayload> FinalizeProjectImageUploads(
        FinalizeProjectImageUploadsInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var project = await images.FinalizeImageUploadAsync(input, ct);

        return new FinalizeProjectImageUploadsPayload(project);
    }

    public async Task<DeleteProjectImagesPayload> DeleteProjectImages(
        DeleteProjectImagesInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var project = await images.DeleteProjectImagesAsync(input, ct);

        return new DeleteProjectImagesPayload(project);
    }
}