namespace Portfolio.Api.GraphQL.Projects.Inputs;

public sealed record ProjectImageUploadRequestInput(
    string ClientId,
    string AltText,
    string FullContentType,
    int FullSizeBytes,
    string ThumbContentType,
    int ThumbSizeBytes
);

public record RequestProjectImageUploadsInput(
    Guid ProjectId,
    IReadOnlyList<ProjectImageUploadRequestInput> Items
);
