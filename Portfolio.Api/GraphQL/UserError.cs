namespace Portfolio.Api.GraphQL;

public enum UserErrorCode
{
    Validation,
    NotFound,
    Conflict,
    InvalidReference,
    InvalidState
}

public sealed record UserError(
    UserErrorCode Code,
    string Message,
    IReadOnlyList<string>? Field = null);
