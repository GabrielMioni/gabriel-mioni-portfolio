namespace Portfolio.Api.GraphQL.Projects.Inputs;

public enum ProjectImageUploadVariant
{
    Full,
    Thumb
}

public record ProjectImageUploadRequestItemInput(
    string ImageClientId,
    ProjectImageUploadVariant Variant,
    string ContentType,
    long SizeBytes
);

public record RequestProjectImageUploadsInput(
    Guid ProjectId,
    IReadOnlyList<ProjectImageUploadRequestItemInput> Items
);
