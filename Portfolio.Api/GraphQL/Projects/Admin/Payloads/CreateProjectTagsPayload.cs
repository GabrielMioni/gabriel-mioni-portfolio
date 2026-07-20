using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record CreateProjectTagsPayload(
    IReadOnlyList<ProjectTag>? Tags,
    IReadOnlyList<UserError> UserErrors);
