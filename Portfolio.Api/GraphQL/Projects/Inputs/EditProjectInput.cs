using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Inputs;

public record EditProjectImageInput(
  Guid ProjectImageId,
  string AltText,
  int SortOrder
);

public record EditProjectLinkInput(
    Guid? Id,
    string Url,
    string LinkText,
    ProjectLinkType LinkType,
    int SortOrder
);

public sealed record EditProjectInput(
    Guid Id,
    string? Title,
    string? Summary,
    string? Body,
    ProjectStatus? Status,
    IReadOnlyList<EditProjectImageInput>? Images,
    IReadOnlyList<EditProjectLinkInput>? Links,
    IReadOnlyList<Guid>? RemovedLinkIds
);
