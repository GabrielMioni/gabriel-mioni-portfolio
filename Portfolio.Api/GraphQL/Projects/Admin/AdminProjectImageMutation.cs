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
        var userErrors = ProjectImageInputValidator.ValidatePrepare(input);

        if (userErrors.Count > 0)
        {
            return new PrepareProjectImageUploadsPayload(
                Items: null,
                UserErrors: userErrors);
        }

        var result = await images.PrepareImageUploadAsync(input, ct);

        if (result.ProjectWasNotFound)
        {
            return new PrepareProjectImageUploadsPayload(
                Items: null,
                UserErrors:
                [
                    UserError.NotFound(
                        $"Project '{input.ProjectId}' was not found.",
                        "input", "projectId")
                ]);
        }

        if (result.Items is null)
            throw new InvalidOperationException("Successful upload preparation returned no instructions.");

        return new PrepareProjectImageUploadsPayload(
            Items: result.Items,
            UserErrors: []);
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