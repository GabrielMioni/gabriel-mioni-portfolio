namespace Portfolio.Api.GraphQL.Projects.Types;

using Portfolio.Api.GraphQL.Projects.Inputs;

public record ProjectImageUploadInstruction(
    string ImageClientId,
    ProjectImageUploadVariant Variant,
    string Key,
    string UploadUrl,
    string PublicUrl
);

public record RequestProjectImageUploadsPayload(
    IReadOnlyList<ProjectImageUploadInstruction> Items
);
