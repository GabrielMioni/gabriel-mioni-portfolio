using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Inputs;

public sealed record CreateProjectLinkInput(
    string Link,
    string LinkText,
    ProjectLinkType LinkType
);

public sealed record CreateProjectInput(
    string Title,
    string? Summary,
    string? Body,
    ProjectStatus Status,
    IEnumerable<CreateProjectLinkInput>? Links
);
