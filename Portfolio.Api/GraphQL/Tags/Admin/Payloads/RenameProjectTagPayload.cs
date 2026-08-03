using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Tags.Admin.Payloads;

public sealed record RenameProjectTagPayload(
    ProjectTag? Tag,
    IReadOnlyList<UserError> UserErrors);
