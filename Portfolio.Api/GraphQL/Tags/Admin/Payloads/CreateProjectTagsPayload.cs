using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Tags.Admin.Payloads;

public sealed record CreateProjectTagsPayload(
    IReadOnlyList<ProjectTag>? Tags,
    IReadOnlyList<UserError> UserErrors);
