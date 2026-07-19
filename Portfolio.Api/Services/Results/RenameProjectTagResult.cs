using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Results;

public enum RenameProjectTagOutcome
{
    Success,
    NotFound,
    Conflict
}

public sealed record RenameProjectTagResult(
    RenameProjectTagOutcome Outcome,
    ProjectTag? Tag)
{
    public static RenameProjectTagResult Success(ProjectTag tag) =>
        new(RenameProjectTagOutcome.Success, tag);

    public static RenameProjectTagResult NotFound() =>
        new(RenameProjectTagOutcome.NotFound, Tag: null);

    public static RenameProjectTagResult Conflict() =>
        new(RenameProjectTagOutcome.Conflict, Tag: null);
}
