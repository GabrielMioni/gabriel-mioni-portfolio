using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;

namespace Portfolio.Api.Services;

public class ProjectTagService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProjectTagService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<ProjectTagSummary>> GetSummariesAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Tags
            .Select(t => new ProjectTagSummary(t.Id, t.Name, t.Value, t.Projects.Count))
            .ToListAsync(ct);
    }

    public async Task<Guid?> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag is null) return null;

        db.Tags.Remove(tag);
        await db.SaveChangesAsync(ct);

        return id;
    }

    public async Task<ProjectTag?> RenameAsync(Guid id, string name, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag is null) return null;

        var newValue = ProjectTag.GenerateValue(name);
        var conflict = await db.Tags.AnyAsync(t => t.Value == newValue && t.Id != id, ct);
        if (conflict)
            throw new InvalidOperationException($"A tag with the name '{name.Trim()}' already exists.");

        tag.Rename(name);
        await db.SaveChangesAsync(ct);

        return tag;
    }

    public async Task RemoveTagFromProjectsAsync(Guid tagId, IReadOnlyList<Guid> projectIds, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var projects = await db.Projects
            .Include(p => p.Tags)
            .Where(p => projectIds.Contains(p.Id))
            .ToListAsync(ct);

        foreach (var project in projects)
        {
            var tag = project.Tags.FirstOrDefault(t => t.Id == tagId);
            if (tag is not null) project.RemoveTag(tag);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<List<Project>> GetProjectsByTagIdAsync(Guid tagId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Projects
            .Where(p => p.Tags.Any(t => t.Id == tagId))
            .OrderBy(p => p.Title)
            .ToListAsync(ct);
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
