using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Admin;

internal static class ProjectTagInputValidator
{
    public static IReadOnlyList<UserError> ValidateName(string name)
    {
        var userError = GetNameValidationError(name, "input", "name");

        return userError is null ? [] : [userError];
    }

    public static IReadOnlyList<UserError> ValidateNames(IReadOnlyList<string> names)
    {
        var userErrors = new List<UserError>();
        var seenValues = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < names.Count; index++)
        {
            var name = names[index];
            var field = new[] { "input", "names", index.ToString() };
            var validationError = GetNameValidationError(name, field);

            if (validationError is not null)
            {
                userErrors.Add(validationError);
                continue;
            }

            var value = ProjectTag.GenerateValue(name);

            if (!seenValues.Add(value))
            {
                userErrors.Add(UserError.Conflict(
                    $"Tag name '{name.Trim()}' duplicates another requested tag.",
                    field));
            }
        }

        return userErrors;
    }

    private static UserError? GetNameValidationError(
        string name,
        params string[] field)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return UserError.Validation(
                "Tag name is required.",
                field);
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > ProjectTag.MaxNameLength)
        {
            return UserError.Validation(
                $"Tag name cannot exceed {ProjectTag.MaxNameLength} characters.",
                field);
        }

        if (string.IsNullOrEmpty(ProjectTag.GenerateValue(normalizedName)))
        {
            return UserError.Validation(
                "Tag name must produce a usable value.",
                field);
        }

        return null;
    }
}
