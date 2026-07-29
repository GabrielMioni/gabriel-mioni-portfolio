using HotChocolate.CostAnalysis.Types;
using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Public.Payloads;

public class PublicProjectDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = default!;

    public string? Summary { get; init; }

    public string? Body { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    [ListSize(AssumedSize = Project.MaxImageCount)]
    public IReadOnlyList<PublicProjectImageDto> Images { get; init; } = [];

    public IReadOnlyList<PublicProjectLinkDto> Links { get; init; } = [];

    [ListSize(AssumedSize = Project.MaxTagCount)]
    public IReadOnlyList<PublicProjectTagDto> Tags { get; init; } = [];
}
