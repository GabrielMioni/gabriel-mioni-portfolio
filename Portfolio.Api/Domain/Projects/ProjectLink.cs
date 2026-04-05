using Portfolio.Api.Domain.Projects;
public enum ProjectLinkType
{
    External = 0,
    Repository = 1,
    Demo = 2
}

public class ProjectLink
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = default!;

    public string Link { get; private set; } = default!;
    public string LinkText { get; private set; } = default!;
    public ProjectLinkType LinkType { get; private set; }

    public int SortOrder { get; private set; }

    private ProjectLink() { }

    public static ProjectLink Create(
        Guid projectId,
        string link,
        string linkText,
        ProjectLinkType linkType,
        int sortOrder)
    {
        var normalized = NormalizeUrl(link);

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid URL", nameof(link));
        }

        return new ProjectLink
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Link = normalized,
            LinkText = linkText.Trim(),
            LinkType = linkType,
            SortOrder = sortOrder
        };
    }

    public bool Update(string link, string linkText, ProjectLinkType linkType)
    {
        var normalized = NormalizeUrl(link);
        var newText = linkText.Trim();

        if (Link == normalized && LinkText == newText && LinkType == linkType)
        {
            return false;
        }

        Link = normalized;
        LinkText = newText;
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

    private static string NormalizeUrl(string url)
    {
        url = url.Trim();

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        return url;
    }
}