using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record FinalizeProjectImageUploadsPayload(
    Project? Project,
    IReadOnlyList<UserError> UserErrors);
