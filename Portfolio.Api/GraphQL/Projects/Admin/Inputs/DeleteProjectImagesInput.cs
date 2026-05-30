namespace Portfolio.Api.GraphQL.Projects.Admin.Inputs;

public record DeleteProjectImagesInput(
    Guid ProjectId,
    IReadOnlyList<Guid> ProjectImageIds
);
