namespace Portfolio.Api.GraphQL.Projects.Inputs;

public record FinalizeProjectImageUploadsInput(
    Guid ProjectId,
    IReadOnlyList<Guid> ProjectImageIds
);
