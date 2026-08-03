namespace Portfolio.Api.GraphQL.Tags.Admin.Payloads;

public sealed record DeleteProjectTagPayload(
    Guid? DeletedTagId,
    IReadOnlyList<UserError> UserErrors);
