namespace Portfolio.Api.GraphQL.Projects.Admin.Inputs;

public sealed record UpdateProjectTagsInput(Guid ProjectId, IReadOnlyList<Guid> TagIds);
