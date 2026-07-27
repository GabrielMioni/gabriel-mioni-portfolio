using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Tags.Results;

public sealed record CreateProjectTagConflict(
    int InputIndex,
    string Name);

public sealed record CreateProjectTagsResult(
    IReadOnlyList<ProjectTag>? Tags,
    IReadOnlyList<CreateProjectTagConflict> Conflicts)
{
    public static CreateProjectTagsResult Success(IReadOnlyList<ProjectTag> tags) =>
        new(tags, Conflicts: []);

    public static CreateProjectTagsResult Conflict(
        IReadOnlyList<CreateProjectTagConflict> conflicts) =>
        new(Tags: null, conflicts);
}
