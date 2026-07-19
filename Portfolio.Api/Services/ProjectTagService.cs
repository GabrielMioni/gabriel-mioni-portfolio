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

    public async Task<RemoveTagFromProjectsResult> RemoveTagFromProjectsAsync(
        Guid tagId,
        IReadOnlyList<Guid> projectIds,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var tagExists = await db.Tags.AnyAsync(tag => tag.Id == tagId, ct);

        if (!tagExists)
            return RemoveTagFromProjectsResult.NotFound();

        var distinctProjectIds = projectIds
            .Distinct()
            .ToArray();

        if (distinctProjectIds.Length == 0)
            return RemoveTagFromProjectsResult.Success([]);

        var projects = await db.Projects
            .Include(project => project.Tags)
            .Where(project => distinctProjectIds.Contains(project.Id))
            .ToListAsync(ct);

        var foundProjectIds = projects
            .Select(project => project.Id)
            .ToHashSet();

        var invalidReferences = projectIds
            .Select((projectId, index) => new InvalidTagProjectReference(index, projectId))
            .Where(reference => !foundProjectIds.Contains(reference.Id))
            .ToArray();

        if (invalidReferences.Length > 0)
            return RemoveTagFromProjectsResult.InvalidReference(invalidReferences);

        var changed = false;

        foreach (var project in projects)
        {
            var tag = project.Tags.FirstOrDefault(t => t.Id == tagId);

            if (tag is null)
                continue;

            project.RemoveTag(tag);
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync(ct);

        return RemoveTagFromProjectsResult.Success(distinctProjectIds);
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

    public async Task<UpdateProjectTagsResult> UpdateProjectTagsAsync(
        Guid projectId,
        IReadOnlyList<Guid> tagIds,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project is null)
            return UpdateProjectTagsResult.NotFound();

        var desiredTagIds = tagIds.ToHashSet();

        var tags = await db.Tags
            .Where(tag => desiredTagIds.Contains(tag.Id))
            .ToListAsync(ct);

        var foundTagIds = tags
            .Select(tag => tag.Id)
            .ToHashSet();

        var invalidReferences = tagIds
            .Select((tagId, index) => new InvalidProjectTagReference(index, tagId))
            .Where(reference => !foundTagIds.Contains(reference.Id))
            .ToArray();

        if (invalidReferences.Length > 0)
            return UpdateProjectTagsResult.InvalidReference(invalidReferences);

        var changed = false;

        foreach (var tag in project.Tags.Where(tag => !desiredTagIds.Contains(tag.Id)).ToList())
        {
            project.RemoveTag(tag);
            changed = true;
        }

        var currentTagIds = project.Tags
            .Select(tag => tag.Id)
            .ToHashSet();

        foreach (var tag in tags.Where(tag => !currentTagIds.Contains(tag.Id)))
        {
            project.AddTag(tag);
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync(ct);

        return UpdateProjectTagsResult.Success(project);
    }
}
