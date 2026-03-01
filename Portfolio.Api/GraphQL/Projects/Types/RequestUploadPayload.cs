namespace Portfolio.Api.GraphQL.Projects.Types;

public record RequestUploadPayload(
    string Key,
    string UploadUrl,
    string PublicUrl
);
