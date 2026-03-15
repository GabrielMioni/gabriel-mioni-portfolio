namespace Portfolio.Api.GraphQL.Projects.Inputs;

public record DeleteProjectImagesInput(
    Guid ProjectId,
    IReadOnlyList<Guid> ProjectImageIds
);
