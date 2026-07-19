using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;

namespace Portfolio.Api.GraphQL.Projects.Admin;

internal static class ProjectInputValidator
{
    public static IReadOnlyList<UserError> ValidateCreate(CreateProjectInput input)
    {
        var userErrors = new List<UserError>();

        ValidateTitle(input.Title, userErrors);
        ValidateLinks(input.Links, userErrors);

        return userErrors;
    }

    private static void ValidateTitle(string title, ICollection<UserError> userErrors)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            userErrors.Add(ValidationError(
                "Title is required.",
                "input", "title"));
            return;
        }

        if (title.Trim().Length > Project.MaxTitleLength)
        {
            userErrors.Add(ValidationError(
                $"Title cannot exceed {Project.MaxTitleLength} characters.",
                "input", "title"));
        }
    }

    private static void ValidateLinks(
        IEnumerable<CreateProjectLinkInput>? links,
        ICollection<UserError> userErrors)
    {
        if (links is null)
            return;

        var index = 0;

        foreach (var link in links)
        {
            var normalizedUrl = ProjectLink.NormalizeUrl(link.Url);
            var fieldPrefix = new[] { "input", "links", index.ToString() };

            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out _))
            {
                userErrors.Add(ValidationError(
                    "Link URL must be a valid absolute URL.",
                    [.. fieldPrefix, "url"]));
            }
            else if (normalizedUrl.Length > ProjectLink.MaxUrlLength)
            {
                userErrors.Add(ValidationError(
                    $"Link URL cannot exceed {ProjectLink.MaxUrlLength} characters.",
                    [.. fieldPrefix, "url"]));
            }

            if (link.LinkText.Trim().Length > ProjectLink.MaxLinkTextLength)
            {
                userErrors.Add(ValidationError(
                    $"Link text cannot exceed {ProjectLink.MaxLinkTextLength} characters.",
                    [.. fieldPrefix, "linkText"]));
            }

            index++;
        }
    }

    private static UserError ValidationError(string message, params string[] field)
    {
        return new UserError(UserErrorCode.Validation, message, field);
    }
}
