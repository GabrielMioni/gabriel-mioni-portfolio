using HotChocolate;

namespace Portfolio.Api.GraphQL;

public sealed class UnexpectedGraphQlErrorFilter(
    ILogger<UnexpectedGraphQlErrorFilter> logger) : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is null)
            return error;

        var errorId = Guid.NewGuid().ToString("N");

        logger.LogError(
            error.Exception,
            "Unexpected GraphQL error {ErrorId} at path {Path}",
            errorId,
            error.Path?.ToString() ?? "<unknown>");

        var sanitizedError = ErrorBuilder.New()
            .SetMessage("An unexpected error occurred.")
            .SetCode("INTERNAL_SERVER_ERROR")
            .SetExtension("errorId", errorId);

        if (error.Path is not null)
            sanitizedError.SetPath(error.Path);

        return sanitizedError.Build();
    }
}
