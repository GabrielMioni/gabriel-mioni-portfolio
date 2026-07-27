namespace Portfolio.Api.Services.Tags.Results;

public sealed record InvalidTagProjectReference(
    int InputIndex,
    Guid Id);

public sealed record RemoveTagFromProjectsResult(
    IReadOnlyList<Guid>? ProjectIds,
    bool TagWasNotFound,
    IReadOnlyList<InvalidTagProjectReference> InvalidReferences)
{
    public static RemoveTagFromProjectsResult Success(IReadOnlyList<Guid> projectIds) =>
        new(projectIds, TagWasNotFound: false, InvalidReferences: []);

    public static RemoveTagFromProjectsResult NotFound() =>
        new(ProjectIds: null, TagWasNotFound: true, InvalidReferences: []);

    public static RemoveTagFromProjectsResult InvalidReference(
        IReadOnlyList<InvalidTagProjectReference> invalidReferences) =>
        new(ProjectIds: null, TagWasNotFound: false, invalidReferences);
}
