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
                UserError.Validation(
                    "Tag name is required.",
                    "input", "name")
            ];
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > ProjectTag.MaxNameLength)
        {
            return
            [
                UserError.Validation(
                    $"Tag name cannot exceed {ProjectTag.MaxNameLength} characters.",
                    "input", "name")
            ];
        }

        if (string.IsNullOrEmpty(ProjectTag.GenerateValue(normalizedName)))
        {
            return
            [
                UserError.Validation(
                    "Tag name must produce a usable value.",
                    "input", "name")
            ];
        }

        return [];
    }
}
