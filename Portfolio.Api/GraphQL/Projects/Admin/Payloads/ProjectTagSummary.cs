namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record ProjectTagSummary(
    Guid Id,
    string Name,
    string Value,
    int ProjectsCount
);
