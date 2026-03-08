namespace Portfolio.Api.GraphQL.Projects.Types;

public record ProjectImageUploadInstruction(
    string ClientId,
    Guid ProjectImageId,
    ProjectImageUploadTarget Full,
    ProjectImageUploadTarget Thumb
);

public record ProjectImageUploadTarget(
    string Key,
    string UploadUrl,
    string PublicUrl,
    string ContentType
);

public record RequestProjectImageUploadsPayload(
    Guid ProjectId,
    IReadOnlyList<ProjectImageUploadInstruction> Items
);
