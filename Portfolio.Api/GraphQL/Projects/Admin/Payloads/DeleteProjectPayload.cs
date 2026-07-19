namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record DeleteProjectPayload(
    Guid? DeletedProjectId,
    IReadOnlyList<UserError> UserErrors);
