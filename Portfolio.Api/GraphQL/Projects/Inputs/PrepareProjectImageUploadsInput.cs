namespace Portfolio.Api.GraphQL.Projects.Inputs;

public sealed record ProjectImagePrepareItem(
    string ClientId,
    string AltText,
    string FullContentType,
    int FullSizeBytes,
    string ThumbContentType,
    int ThumbSizeBytes,
    int Height,
    int Width
);

public record PrepareProjectImageUploadsInput(
    Guid ProjectId,
    IReadOnlyList<ProjectImagePrepareItem> Items
);
