namespace Portfolio.Api.GraphQL.Projects.Public.Payloads
{
    public class PublicProjectLinkDto
    {
        public Guid Id { get; init; }

        public string Url { get; init; } = default!;

        public string LinkText { get; init; } = default!;

        public ProjectLinkType LinkType { get; init; }

        public int SortOrder { get; init; }
    }
}
