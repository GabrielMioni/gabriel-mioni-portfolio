using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Results;

public sealed record InvalidProjectTagReference(
    int InputIndex,
    Guid Id);

public sealed record UpdateProjectTagsResult(
    Project? Project,
    bool ProjectWasNotFound,
    IReadOnlyList<InvalidProjectTagReference> InvalidReferences)
{
    public static UpdateProjectTagsResult Success(Project project) =>
        new(project, ProjectWasNotFound: false, InvalidReferences: []);

    public static UpdateProjectTagsResult NotFound() =>
        new(Project: null, ProjectWasNotFound: true, InvalidReferences: []);

    public static UpdateProjectTagsResult InvalidReference(
        IReadOnlyList<InvalidProjectTagReference> invalidReferences) =>
        new(Project: null, ProjectWasNotFound: false, invalidReferences);
}
