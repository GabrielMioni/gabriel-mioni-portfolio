using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record CreateProjectPayload(
    Project? Project,
    IReadOnlyList<UserError> UserErrors);
