using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Results;

public enum EditProjectReferenceKind
{
    Image,
    Link
}

public sealed record InvalidEditProjectReference(
    EditProjectReferenceKind Kind,
    int InputIndex,
    Guid Id);

public sealed record EditProjectResult(
    Project? Project,
    bool ProjectWasNotFound,
    IReadOnlyList<InvalidEditProjectReference> InvalidReferences)
{
    public static EditProjectResult Success(Project project) =>
        new(project, ProjectWasNotFound: false, InvalidReferences: []);

    public static EditProjectResult NotFound() =>
        new(Project: null, ProjectWasNotFound: true, InvalidReferences: []);

    public static EditProjectResult InvalidReference(
        IReadOnlyList<InvalidEditProjectReference> invalidReferences) =>
        new(Project: null, ProjectWasNotFound: false, invalidReferences);
}
