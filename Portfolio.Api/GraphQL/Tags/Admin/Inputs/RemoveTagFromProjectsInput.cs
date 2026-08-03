namespace Portfolio.Api.GraphQL.Tags.Admin.Inputs;

public sealed record RemoveTagFromProjectsInput(Guid TagId, IReadOnlyList<Guid> ProjectIds);
