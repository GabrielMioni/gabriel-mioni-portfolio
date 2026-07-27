namespace Portfolio.Api.Services.Images.Results;

public sealed record InvalidProjectImageReference(
    int InputIndex,
    Guid Id);
