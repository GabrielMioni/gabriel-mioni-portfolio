using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services.Results;

namespace Portfolio.Api.Services;

public class ProjectTagService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProjectTagService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<ProjectTagSummary>> GetSummariesAsync(bool showOrphaned = true, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var query = db.Tags.AsQueryable();
        if (!showOrphaned)
            query = query.Where(t => t.Projects.Any());
        return await query
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

    public async Task<RenameProjectTagResult> RenameAsync(
        Guid id,
        string name,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag is null)
            return RenameProjectTagResult.NotFound();

        var newValue = ProjectTag.GenerateValue(name);
        var conflict = await db.Tags.AnyAsync(t => t.Value == newValue && t.Id != id, ct);
        if (conflict)
            return RenameProjectTagResult.Conflict();

        tag.Rename(name);
        await db.SaveChangesAsync(ct);

        return RenameProjectTagResult.Success(tag);
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

    public async Task<CreateProjectTagsResult> CreateManyAsync(
        IReadOnlyList<string> names,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        if (names.Count == 0)
            return CreateProjectTagsResult.Success([]);

        var candidates = names
            .Select((name, index) => new
            {
                Index = index,
                Name = name,
                Value = ProjectTag.GenerateValue(name)
            })
            .ToArray();

        var duplicateConflicts = candidates
            .GroupBy(candidate => candidate.Value, StringComparer.Ordinal)
            .SelectMany(group => group.Skip(1))
            .Select(candidate => new CreateProjectTagConflict(
                candidate.Index,
                candidate.Name));

        var values = candidates
            .Select(candidate => candidate.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var existingValueList = await db.Tags
            .Where(tag => values.Contains(tag.Value))
            .Select(tag => tag.Value)
            .ToListAsync(ct);

        var existingValues = existingValueList.ToHashSet(StringComparer.Ordinal);

        var existingConflicts = candidates
            .Where(candidate => existingValues.Contains(candidate.Value))
            .Select(candidate => new CreateProjectTagConflict(
                candidate.Index,
                candidate.Name));

        var conflicts = duplicateConflicts
            .Concat(existingConflicts)
            .DistinctBy(conflict => conflict.InputIndex)
            .OrderBy(conflict => conflict.InputIndex)
            .ToArray();

        if (conflicts.Length > 0)
            return CreateProjectTagsResult.Conflict(conflicts);

        var created = names
            .Select(ProjectTag.Create)
            .ToArray();

        db.Tags.AddRange(created);

        await db.SaveChangesAsync(ct);

        return CreateProjectTagsResult.Success(created);
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
