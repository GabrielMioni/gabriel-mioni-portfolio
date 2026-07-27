using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Images.Results;

public sealed record DeleteProjectImagesResult(
    Project? Project,
    bool ProjectWasNotFound,
    IReadOnlyList<InvalidProjectImageReference> InvalidReferences)
{
    public static DeleteProjectImagesResult Success(Project project) =>
        new(
            project,
            ProjectWasNotFound: false,
            InvalidReferences: []);

    public static DeleteProjectImagesResult NotFound() =>
        new(
            Project: null,
            ProjectWasNotFound: true,
            InvalidReferences: []);

    public static DeleteProjectImagesResult InvalidReference(
        IReadOnlyList<InvalidProjectImageReference> invalidReferences) =>
        new(
            Project: null,
            ProjectWasNotFound: false,
            invalidReferences);
}
