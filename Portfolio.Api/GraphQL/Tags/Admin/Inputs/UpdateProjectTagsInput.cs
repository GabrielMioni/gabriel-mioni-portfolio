namespace Portfolio.Api.GraphQL.Tags.Admin.Inputs;

public sealed record UpdateProjectTagsInput(Guid ProjectId, IReadOnlyList<Guid> TagIds);
