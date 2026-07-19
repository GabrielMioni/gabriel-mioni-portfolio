using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Results;

public sealed record FinalizeProjectImageUploadsResult(
    Project? Project,
    bool ProjectWasNotFound,
    IReadOnlyList<InvalidProjectImageReference> InvalidReferences)
{
    public static FinalizeProjectImageUploadsResult Success(Project project) =>
        new(project, ProjectWasNotFound: false, InvalidReferences: []);

    public static FinalizeProjectImageUploadsResult NotFound() =>
        new(Project: null, ProjectWasNotFound: true, InvalidReferences: []);

    public static FinalizeProjectImageUploadsResult InvalidReference(
        IReadOnlyList<InvalidProjectImageReference> invalidReferences) =>
        new(Project: null, ProjectWasNotFound: false, invalidReferences);
}
