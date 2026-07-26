namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record DeleteProjectsPayload(
    IReadOnlyList<Guid>? DeletedProjectIds,
    IReadOnlyList<UserError> UserErrors);
