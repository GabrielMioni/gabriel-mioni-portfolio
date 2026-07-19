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
        var result = await images.FinalizeImageUploadAsync(input, ct);

        if (result.ProjectWasNotFound)
        {
            return new FinalizeProjectImageUploadsPayload(
                Project: null,
                UserErrors:
                [
                    UserError.NotFound(
                        $"Project '{input.ProjectId}' was not found.",
                        "input", "projectId")
                ]);
        }

        if (result.InvalidReferences.Count > 0)
        {
            return new FinalizeProjectImageUploadsPayload(
                Project: null,
                UserErrors: result.InvalidReferences
                    .Select(reference => UserError.InvalidReference(
                        $"Project image '{reference.Id}' was not found on this project.",
                        "input", "projectImageIds", reference.InputIndex.ToString()))
                    .ToArray());
        }

        if (result.Project is null)
            throw new InvalidOperationException("Successful upload finalization returned no project.");

        return new FinalizeProjectImageUploadsPayload(
            Project: result.Project,
            UserErrors: []);
    }

    public async Task<DeleteProjectImagesPayload> DeleteProjectImages(
        DeleteProjectImagesInput input,
        [Service] ProjectImageService images,
        CancellationToken ct)
    {
        var result = await images.DeleteProjectImagesAsync(input, ct);

        if (result.ProjectWasNotFound)
        {
            return new DeleteProjectImagesPayload(
                Project: null,
                UserErrors:
                [
                    UserError.NotFound(
                        $"Project '{input.ProjectId}' was not found.",
                        "input", "projectId")
                ]);
        }

        if (result.Project is null)
            throw new InvalidOperationException("Successful image deletion returned no project.");

        return new DeleteProjectImagesPayload(
            Project: result.Project,
            UserErrors: []);
    }
}
