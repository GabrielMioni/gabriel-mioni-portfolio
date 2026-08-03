namespace Portfolio.Api.GraphQL.Tags.Admin.Payloads;

public sealed record RemoveTagFromProjectsPayload(
    IReadOnlyList<Guid>? ProjectIds,
    IReadOnlyList<UserError> UserErrors);
