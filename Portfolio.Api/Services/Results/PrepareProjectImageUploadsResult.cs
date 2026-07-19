using Portfolio.Api.GraphQL.Projects.Admin.Payloads;

namespace Portfolio.Api.Services.Results;

public sealed record PrepareProjectImageUploadsResult(
    IReadOnlyList<ProjectImageUploadInstruction>? Items,
    bool ProjectWasNotFound)
{
    public static PrepareProjectImageUploadsResult Success(
        IReadOnlyList<ProjectImageUploadInstruction> items) =>
        new(items, ProjectWasNotFound: false);

    public static PrepareProjectImageUploadsResult NotFound() =>
        new(Items: null, ProjectWasNotFound: true);
}
