namespace Portfolio.Api.GraphQL.Tags.Admin.Inputs;

public sealed record CreateProjectTagsInput(IReadOnlyList<string> Names);
