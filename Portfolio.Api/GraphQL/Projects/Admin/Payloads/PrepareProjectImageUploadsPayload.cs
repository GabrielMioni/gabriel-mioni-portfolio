namespace Portfolio.Api.GraphQL.Projects.Admin.Payloads;

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
    IReadOnlyList<ProjectImageUploadInstruction>? Items,
    IReadOnlyList<UserError> UserErrors
);
