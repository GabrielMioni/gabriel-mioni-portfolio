namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record RemoveTagFromProjectsPayload(
    IReadOnlyList<Guid>? ProjectIds,
    IReadOnlyList<UserError> UserErrors);
