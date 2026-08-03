namespace Portfolio.Api.GraphQL.Tags.Admin.Payloads;

public sealed record ProjectTagSummary(
    Guid Id,
    string Name,
    string Value,
    int ProjectsCount
);
