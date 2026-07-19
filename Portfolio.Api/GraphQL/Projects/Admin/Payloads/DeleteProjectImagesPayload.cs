using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

public sealed record DeleteProjectImagesPayload(
    Project? Project,
    IReadOnlyList<UserError> UserErrors);
