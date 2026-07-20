using Portfolio.Api.Domain.Projects;
public enum ProjectLinkType
{
    External = 0,
    Repository = 1,
    Demo = 2
}

public class ProjectLink
{
    public const int MaxUrlLength = 2048;
    public const int MaxLinkTextLength = 300;

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = default!;

    public string Url { get; private set; } = default!;
    public string LinkText { get; private set; } = default!;
    public ProjectLinkType LinkType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int SortOrder { get; private set; }

    private ProjectLink() { }

    public static ProjectLink Create(
        Guid projectId,
        string url,
        string linkText,
        ProjectLinkType linkType,
        int sortOrder)
    {
        var (normalizedUrl, normalizedLinkText) = NormalizeAndValidate(url, linkText);

        return new ProjectLink
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Url = normalizedUrl,
            LinkText = normalizedLinkText,
            LinkType = linkType,
            CreatedAt = DateTime.UtcNow,
            SortOrder = sortOrder
        };
    }

    public bool Update(string url, string linkText, ProjectLinkType linkType)
    {
        var (normalizedUrl, normalizedLinkText) = NormalizeAndValidate(url, linkText);

        if (Url == normalizedUrl &&
            LinkText == normalizedLinkText &&
            LinkType == linkType)
        {
            return false;
        }

        Url = normalizedUrl;
        LinkText = normalizedLinkText;
        LinkType = linkType;

        return true;
    }

    public bool UpdateSortOrder(int sortOrder)
    {
        if (SortOrder == sortOrder)
            return false;

        SortOrder = sortOrder;
        return true;
    }

    public static string NormalizeUrl(string url)
    {
        url = url.Trim();

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        return url;
    }

    private static (string Url, string LinkText) NormalizeAndValidate(
        string url,
        string linkText)
    {
        var normalizedUrl = NormalizeUrl(url);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid URL", nameof(url));

        if (normalizedUrl.Length > MaxUrlLength)
        {
            throw new ArgumentException(
                $"URL cannot exceed {MaxUrlLength} characters.",
                nameof(url));
        }

        var normalizedLinkText = linkText.Trim();

        if (normalizedLinkText.Length > MaxLinkTextLength)
        {
            throw new ArgumentException(
                $"Link text cannot exceed {MaxLinkTextLength} characters.",
                nameof(linkText));
        }

        return (normalizedUrl, normalizedLinkText);
    }
}
