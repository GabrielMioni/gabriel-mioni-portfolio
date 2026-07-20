using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Results;

public sealed record IncompleteProjectImageUpload(
    int InputIndex,
    Guid Id,
    bool FullImageWasMissing,
    bool ThumbnailWasMissing);

public sealed record FinalizeProjectImageUploadsResult(
    Project? Project,
    bool ProjectWasNotFound,
    IReadOnlyList<InvalidProjectImageReference> InvalidReferences,
    IReadOnlyList<IncompleteProjectImageUpload> IncompleteUploads)
{
    public static FinalizeProjectImageUploadsResult Success(Project project) =>
        new(
            project,
            ProjectWasNotFound: false,
            InvalidReferences: [],
            IncompleteUploads: []);

    public static FinalizeProjectImageUploadsResult NotFound() =>
        new(
            Project: null,
            ProjectWasNotFound: true,
            InvalidReferences: [],
            IncompleteUploads: []);

    public static FinalizeProjectImageUploadsResult InvalidReference(
        IReadOnlyList<InvalidProjectImageReference> invalidReferences) =>
        new(
            Project: null,
            ProjectWasNotFound: false,
            invalidReferences,
            IncompleteUploads: []);

    public static FinalizeProjectImageUploadsResult IncompleteUpload(
        IReadOnlyList<IncompleteProjectImageUpload> incompleteUploads) =>
        new(
            Project: null,
            ProjectWasNotFound: false,
            InvalidReferences: [],
            incompleteUploads);
}
