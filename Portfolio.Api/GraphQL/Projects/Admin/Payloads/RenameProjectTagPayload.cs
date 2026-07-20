using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record RenameProjectTagPayload(
    ProjectTag? Tag,
    IReadOnlyList<UserError> UserErrors);
