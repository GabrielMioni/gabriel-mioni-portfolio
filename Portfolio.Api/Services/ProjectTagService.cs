using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services;

public class ProjectTagService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProjectTagService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<ProjectTag>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ProjectTag>> CreateManyAsync(IReadOnlyList<string> names, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var created = new List<ProjectTag>();

        foreach (var name in names)
        {
            var value = ProjectTag.GenerateValue(name);

            var exists = await db.Tags.AnyAsync(t => t.Value == value, ct);
            if (exists)
                throw new InvalidOperationException($"A tag with the name '{name.Trim()}' already exists.");

            var tag = ProjectTag.Create(name);
            db.Tags.Add(tag);
            created.Add(tag);
        }

        await db.SaveChangesAsync(ct);

        return created;
    }

    public async Task<Project?> UpdateProjectTagsAsync(
        Guid projectId,
        IReadOnlyList<Guid> tagIds,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project is null)
            return null;

        var tags = await db.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(ct);

        foreach (var tag in project.Tags.ToList())
            project.RemoveTag(tag);

        foreach (var tag in tags)
            project.AddTag(tag);

        await db.SaveChangesAsync(ct);

        return project;
    }
}
