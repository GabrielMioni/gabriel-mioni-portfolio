using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Admin;

internal static class ProjectTagInputValidator
{
    public static IReadOnlyList<UserError> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return
            [
                ValidationError("Tag name is required.")
            ];
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > ProjectTag.MaxNameLength)
        {
            return
            [
                ValidationError(
                    $"Tag name cannot exceed {ProjectTag.MaxNameLength} characters.")
            ];
        }

        if (string.IsNullOrEmpty(ProjectTag.GenerateValue(normalizedName)))
        {
            return
            [
                ValidationError("Tag name must produce a usable value.")
            ];
        }

        return [];
    }

    private static UserError ValidationError(string message)
    {
        return new UserError(
            UserErrorCode.Validation,
            message,
            ["input", "name"]);
    }
}
