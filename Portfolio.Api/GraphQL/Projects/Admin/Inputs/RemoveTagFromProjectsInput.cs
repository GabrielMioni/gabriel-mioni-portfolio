namespace Portfolio.Api.GraphQL.Projects.Admin.Inputs;

public sealed record RemoveTagFromProjectsInput(Guid TagId, IReadOnlyList<Guid> ProjectIds);
