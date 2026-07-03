namespace Portfolio.Api.GraphQL.Projects.Admin.Inputs;

public sealed record CreateProjectTagsInput(IReadOnlyList<string> Names);
