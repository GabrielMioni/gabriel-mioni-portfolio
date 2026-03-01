using Portfolio.Api.Domain.Projects;

public class ProjectImage
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = default!;

    public string FullKey { get; private set; } = default!;
    public string ThumbKey { get; private set; } = default!;

    public int SortOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private ProjectImage() { } // EF

    public ProjectImage(
      Guid projectId,
      string fullKey,
      string thumbKey,
      int sortOrder)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        FullKey = fullKey;
        ThumbKey = thumbKey;
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
    }
}