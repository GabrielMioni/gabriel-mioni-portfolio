namespace Portfolio.Api.Domain.Projects;

public class Project
{
    public const int MaxTitleLength = 300;
    public const int MaxImageCount = 6;
    public const int MaxTagCount = 15;

    public Guid Id { get; private set; }

    public string Title { get; private set; } = default!;
    public string? Summary { get; private set; }
    public string? Body { get; private set; }

    public DateTimeOffset? CreatedAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public ProjectStatus? Status { get; private set; }

    public ICollection<ProjectImage> Images { get; private set; } = new List<ProjectImage>();
    public ICollection<ProjectLink> Links { get; private set; } = new List<ProjectLink>();
    public ICollection<ProjectTag> Tags { get; private set; } = new List<ProjectTag>();

    private Project() { } // EF

    public static Project Create(
        string title,
        string? summary,
        string? body,
        ProjectStatus status = ProjectStatus.Draft)
    {
        var now = DateTimeOffset.UtcNow;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = NormalizeTitle(title),
            Summary = NormalizeOptionalTrimmed(summary),
            Body = NormalizeBody(body),
            CreatedAt = now,
            UpdatedAt = now,
            Status = status
        };

        if (status == ProjectStatus.Published)
        {
            project.PublishedAt = now;
        }

        return project;
    }

    public bool UpdateDetails(
        string title,
        string? summary,
        string? body)
    {
        var normalizedTitle = NormalizeTitle(title);
        var normalizedSummary = NormalizeOptionalTrimmed(summary);
        var normalizedBody = NormalizeBody(body);

        if (Title == normalizedTitle &&
            Summary == normalizedSummary &&
            Body == normalizedBody)
        {
            return false;
        }

        Title = normalizedTitle;
        Summary = normalizedSummary;
        Body = normalizedBody;
        Touch();

        return true;
    }

    public bool UpdateStatus(ProjectStatus status)
    {
        if (Status == status)
            return false;

        Status = status;

        if (status == ProjectStatus.Published && PublishedAt is null)
        {
            PublishedAt = DateTimeOffset.UtcNow;
        }

        Touch();

        return true;
    }

    public void AddImage(ProjectImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (Images.Count >= MaxImageCount)
        {
            throw new InvalidOperationException(
                $"A project cannot have more than {MaxImageCount} images.");
        }

        Images.Add(image);
        Touch();
    }

    public void RemoveImage(ProjectImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        Images.Remove(image);
        Touch();
    }

    public void AddTag(ProjectTag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (Tags.Count >= MaxTagCount)
        {
            throw new InvalidOperationException(
                $"A project cannot have more than {MaxTagCount} tags.");
        }

        Tags.Add(tag);
        Touch();
    }

    public void RemoveTag(ProjectTag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        Tags.Remove(tag);
        Touch();
    }

    public void AddLink(ProjectLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.Add(link);
        Touch();
    }

    public void RemoveLink(ProjectLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.Remove(link);
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Value is required.", nameof(title));

        var normalized = title.Trim();

        if (normalized.Length > MaxTitleLength)
            throw new ArgumentException(
                $"Value cannot exceed {MaxTitleLength} characters.",
                nameof(title));

        return normalized;
    }

    private static string? NormalizeOptionalTrimmed(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeBody(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? null
            : value;
    }
}
