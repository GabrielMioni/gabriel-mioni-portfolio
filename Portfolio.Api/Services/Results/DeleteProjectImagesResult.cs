using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Results;

public sealed record DeleteProjectImagesResult(
    Project? Project,
    bool ProjectWasNotFound)
{
    public static DeleteProjectImagesResult Success(Project project) =>
        new(project, ProjectWasNotFound: false);

    public static DeleteProjectImagesResult NotFound() =>
        new(Project: null, ProjectWasNotFound: true);
}
