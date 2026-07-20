namespace Portfolio.Api.Services.Results;

public sealed record InvalidProjectImageReference(
    int InputIndex,
    Guid Id);
