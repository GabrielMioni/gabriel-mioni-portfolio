namespace Portfolio.Api.GraphQL.Projects.Payloads;

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

public record PrepareProjectImageUploadsPayload(
    Guid ProjectId,
    IReadOnlyList<ProjectImageUploadInstruction> Items
);
