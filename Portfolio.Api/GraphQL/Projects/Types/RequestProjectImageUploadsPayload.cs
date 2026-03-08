namespace Portfolio.Api.GraphQL.Projects.Types;

public record ProjectImageUploadInstruction(
    string ClientId,
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
    IReadOnlyList<ProjectImageUploadInstruction> Items
);
