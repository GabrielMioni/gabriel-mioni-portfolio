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
    IReadOnlyList<string>? Field = null)
{
    public static UserError Validation(string message, params string[] field) =>
        Create(UserErrorCode.Validation, message, field);

    public static UserError NotFound(string message, params string[] field) =>
        Create(UserErrorCode.NotFound, message, field);

    public static UserError Conflict(string message, params string[] field) =>
        Create(UserErrorCode.Conflict, message, field);

    public static UserError InvalidReference(string message, params string[] field) =>
        Create(UserErrorCode.InvalidReference, message, field);

    public static UserError InvalidState(string message, params string[] field) =>
        Create(UserErrorCode.InvalidState, message, field);

    private static UserError Create(
        UserErrorCode code,
        string message,
        IReadOnlyList<string> field)
    {
        return new UserError(
            code,
            message,
            field.Count > 0 ? field : null);
    }
}
