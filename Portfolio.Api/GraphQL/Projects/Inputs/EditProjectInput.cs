using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Inputs;

public sealed record EditProjectInput(
    Guid Id,
    string? Title,
    string? Summary,
    string? Body,
    ProjectStatus? Status
);
