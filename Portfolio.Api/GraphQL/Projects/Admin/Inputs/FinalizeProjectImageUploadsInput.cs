namespace Portfolio.Api.GraphQL.Projects.Admin.Inputs;

public record FinalizeProjectImageUploadsInput(
    Guid ProjectId,
    IReadOnlyList<Guid> ProjectImageIds
);
