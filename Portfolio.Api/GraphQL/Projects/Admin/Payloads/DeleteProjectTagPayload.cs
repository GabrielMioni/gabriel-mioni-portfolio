namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record DeleteProjectTagPayload(
    Guid? DeletedTagId,
    IReadOnlyList<UserError> UserErrors);
