namespace Portfolio.Api.Services.Results;

public sealed record InvalidProjectReference(
    int InputIndex,
    Guid Id);

public sealed record DeleteProjectsResult(
    IReadOnlyList<Guid>? DeletedProjectIds,
    IReadOnlyList<InvalidProjectReference> InvalidReferences)
{
    public static DeleteProjectsResult Success(IReadOnlyList<Guid> deletedProjectIds) =>
        new(deletedProjectIds, InvalidReferences: []);

    public static DeleteProjectsResult InvalidReference(
        IReadOnlyList<InvalidProjectReference> invalidReferences) =>
        new(DeletedProjectIds: null, invalidReferences);
}
