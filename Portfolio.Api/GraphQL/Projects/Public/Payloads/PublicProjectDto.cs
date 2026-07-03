namespace Portfolio.Api.GraphQL.Projects.Public.Payloads
{
    public class PublicProjectDto
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = default!;

        public string? Summary { get; init; }

        public string? Body { get; init; }

        public DateTimeOffset? PublishedAt { get; init; }

        public IReadOnlyList<PublicProjectImageDto> Images { get; init; } = [];

        public IReadOnlyList<PublicProjectLinkDto> Links { get; init; } = [];

        public IReadOnlyList<PublicProjectTagDto> Tags { get; init; } = [];
    }
}
