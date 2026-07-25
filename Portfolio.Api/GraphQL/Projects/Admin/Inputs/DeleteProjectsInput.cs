namespace Portfolio.Api.GraphQL.Projects.Admin.Inputs;

public sealed record DeleteProjectsInput(
    IReadOnlyList<Guid> ProjectIds);
