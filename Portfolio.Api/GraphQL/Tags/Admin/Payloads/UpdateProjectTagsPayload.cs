using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Tags.Admin.Payloads;

public sealed record UpdateProjectTagsPayload(
    Project? Project,
    IReadOnlyList<UserError> UserErrors);
