namespace Portfolio.Api.GraphQL.Projects.Public.Payloads
{
    public class PublicProjectImageDto
    {
        public Guid Id { get; init; }

        public string FullKey { get; init; } = default!;

        public string ThumbKey { get; init; } = default!;

        public string? AltText { get; init; }

        public int SortOrder { get; init; }
    }
}
