using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;

namespace Portfolio.Api.GraphQL.Projects.Admin;

internal static class ProjectInputValidator
{
    public static IReadOnlyList<UserError> ValidateCreate(CreateProjectInput input)
    {
        var userErrors = new List<UserError>();

        ValidateTitle(input.Title, userErrors);
        ValidateOptionalLength(
            input.Summary?.Trim(),
            Project.MaxSummaryLength,
            "Summary",
            ["input", "summary"],
            userErrors);
        ValidateOptionalLength(
            input.Body,
            Project.MaxBodyLength,
            "Body",
            ["input", "body"],
            userErrors);
        ValidateLinks(input.Links, userErrors);

        return userErrors;
    }

    public static IReadOnlyList<UserError> ValidateEdit(EditProjectInput input)
    {
        var userErrors = new List<UserError>();

        if (input.Title is not null)
            ValidateTitle(input.Title, userErrors);

        ValidateOptionalLength(
            input.Summary?.Trim(),
            Project.MaxSummaryLength,
            "Summary",
            ["input", "summary"],
            userErrors);
        ValidateOptionalLength(
            input.Body,
            Project.MaxBodyLength,
            "Body",
            ["input", "body"],
            userErrors);
        ValidateImages(input.Images, userErrors);
        ValidateLinks(input.Links, userErrors);

        return userErrors;
    }

    private static void ValidateImages(
        IReadOnlyList<EditProjectImageInput>? images,
        ICollection<UserError> userErrors)
    {
        if (images is null)
            return;

        for (var index = 0; index < images.Count; index++)
        {
            ValidateOptionalLength(
                images[index].AltText.Trim(),
                ProjectImage.MaxAltTextLength,
                "Alt text",
                ["input", "images", index.ToString(), "altText"],
                userErrors);
        }
    }

    private static void ValidateTitle(string title, ICollection<UserError> userErrors)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            userErrors.Add(UserError.Validation(
                "Title is required.",
                "input", "title"));
            return;
        }

        if (title.Trim().Length > Project.MaxTitleLength)
        {
            userErrors.Add(UserError.Validation(
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
            var fieldPrefix = new[] { "input", "links", index.ToString() };
            ValidateLink(link.Url, link.LinkText, fieldPrefix, userErrors);

            index++;
        }
    }

    private static void ValidateLinks(
        IReadOnlyList<EditProjectLinkInput>? links,
        ICollection<UserError> userErrors)
    {
        if (links is null)
            return;

        for (var index = 0; index < links.Count; index++)
        {
            var link = links[index];
            var fieldPrefix = new[] { "input", "links", index.ToString() };
            ValidateLink(link.Url, link.LinkText, fieldPrefix, userErrors);
        }
    }

    private static void ValidateLink(
        string url,
        string linkText,
        IReadOnlyList<string> fieldPrefix,
        ICollection<UserError> userErrors)
    {
        var normalizedUrl = ProjectLink.NormalizeUrl(url);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out _))
        {
            userErrors.Add(UserError.Validation(
                "Link URL must be a valid absolute URL.",
                [.. fieldPrefix, "url"]));
        }
        else if (normalizedUrl.Length > ProjectLink.MaxUrlLength)
        {
            userErrors.Add(UserError.Validation(
                $"Link URL cannot exceed {ProjectLink.MaxUrlLength} characters.",
                [.. fieldPrefix, "url"]));
        }

        if (linkText.Trim().Length > ProjectLink.MaxLinkTextLength)
        {
            userErrors.Add(UserError.Validation(
                $"Link text cannot exceed {ProjectLink.MaxLinkTextLength} characters.",
                [.. fieldPrefix, "linkText"]));
        }
    }

    private static void ValidateOptionalLength(
        string? value,
        int maximumLength,
        string fieldLabel,
        IReadOnlyList<string> field,
        ICollection<UserError> userErrors)
    {
        if (value is null || value.Length <= maximumLength)
            return;

        userErrors.Add(UserError.Validation(
            $"{fieldLabel} cannot exceed {maximumLength} characters.",
            [.. field]));
    }
}
