namespace Portfolio.Api.GraphQL.Projects.Public.Payloads;

public class PublicProjectTagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Value { get; init; } = default!;
}
