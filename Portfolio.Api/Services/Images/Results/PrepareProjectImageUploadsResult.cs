using Portfolio.Api.GraphQL.Projects.Admin.Payloads;

namespace Portfolio.Api.Services.Images.Results;

public sealed record PrepareProjectImageUploadsResult(
    IReadOnlyList<ProjectImageUploadInstruction>? Items,
    bool ProjectWasNotFound,
    bool ImageLimitWasExceeded)
{
    public static PrepareProjectImageUploadsResult Success(
        IReadOnlyList<ProjectImageUploadInstruction> items) =>
        new(
            items,
            ProjectWasNotFound: false,
            ImageLimitWasExceeded: false);

    public static PrepareProjectImageUploadsResult NotFound() =>
        new(
            Items: null,
            ProjectWasNotFound: true,
            ImageLimitWasExceeded: false);

    public static PrepareProjectImageUploadsResult ImageLimitExceeded() =>
        new(
            Items: null,
            ProjectWasNotFound: false,
            ImageLimitWasExceeded: true);
}
