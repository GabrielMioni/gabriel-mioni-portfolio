using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.Services.Helpers;
using Portfolio.Api.Services.Results;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.Services;

public class ProjectService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IObjectStorage _storage;

    public ProjectService(IDbContextFactory<AppDbContext> dbFactory, IObjectStorage storage)
    {
        _dbFactory = dbFactory;
        _storage = storage;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Projects
            .Include(p => p.Images)
            .Include(p => p.Links)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project> CreateAsync(CreateProjectInput input, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var newProject = Project.Create(
            title: input.Title,
            summary: input.Summary,
            body: input.Body,
            status: input.Status);

        db.Projects.Add(newProject);

        AddProjectLinks(newProject, input.Links);

        await db.SaveChangesAsync(ct);

        return newProject;
    }

    public async Task<Guid?> DeleteProjectAsync(
        Guid projectId,
        CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project is null)
            return null;

        var deleteKeys = ProjectImageStorageKeyHelper.GetStorageKeys(project.Images);

        db.Projects.Remove(project);

        await db.SaveChangesAsync(ct);

        await _storage.DeleteImagesAsync(deleteKeys, ct);

        return projectId;
    }

    public async Task<EditProjectResult> EditProjectAsync(EditProjectInput input, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects
            .Include(p => p.Images)
            .Include(p => p.Links)
            .FirstOrDefaultAsync(p => p.Id == input.Id, ct);

        if (project is null)
            return EditProjectResult.NotFound();

        var invalidReferences = GetInvalidEditReferences(project, input);

        if (invalidReferences.Count > 0)
            return EditProjectResult.InvalidReference(invalidReferences);

        var changed = false;

        changed |= UpdateProjectDetails(project, input);
        changed |= UpdateProjectStatus(project, input);
        changed |= UpdateProjectImages(project, input.Images);
        changed |= UpdateProjectLinks(project, input.Links);
        changed |= RemoveProjectLinks(project, input.RemovedLinkIds);

        if (input.Links is not null || input.RemovedLinkIds is not null)
        {
            changed |= NormalizeLinkSortOrder(project);
        }

        if (!changed)
            return EditProjectResult.Success(project);

        try
        {
            Console.WriteLine(db.ChangeTracker.DebugView.LongView);
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine("Concurrency exception during EditProjectAsync");

            foreach (var entry in ex.Entries)
            {
                Console.WriteLine($"Entity: {entry.Entity.GetType().Name}");
                Console.WriteLine($"State: {entry.State}");

                foreach (var prop in entry.Properties)
                {
                    Console.WriteLine(
                        $"  {prop.Metadata.Name}: Current={prop.CurrentValue}, Original={prop.OriginalValue}"
                    );
                }
            }

            throw;
        }

        return EditProjectResult.Success(project);
    }

    public async Task<List<Project>> GetPublishedAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Projects
            .Where(p => p.Status == ProjectStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync(ct);
    }

    public IQueryable<Project> QueryProjects(AppDbContext db, bool includeUnpublished)
    {
        var q = db.Projects.AsQueryable();

        if (!includeUnpublished)
        {
            q = q.Where(p => p.Status == ProjectStatus.Published);
        }

        return q;
    }

    public async Task<Project?> PublishAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project is null)
            return null;

        project.UpdateStatus(ProjectStatus.Published);

        await db.SaveChangesAsync(ct);

        return project;
    }

    public async Task<Project?> ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project is null)
            return null;

        project.UpdateStatus(ProjectStatus.Archived);

        await db.SaveChangesAsync(ct);

        return project;
    }

    private static bool UpdateProjectDetails(Project project, EditProjectInput input)
    {
        return project.UpdateDetails(
            title: input.Title ?? project.Title,
            summary: input.Summary ?? project.Summary,
            body: input.Body ?? project.Body);
    }

    private static IReadOnlyList<InvalidEditProjectReference> GetInvalidEditReferences(
        Project project,
        EditProjectInput input)
    {
        var invalidReferences = new List<InvalidEditProjectReference>();

        if (input.Images is not null)
        {
            for (var index = 0; index < input.Images.Count; index++)
            {
                var imageId = input.Images[index].ProjectImageId;

                if (project.Images.All(image => image.Id != imageId))
                {
                    invalidReferences.Add(new InvalidEditProjectReference(
                        EditProjectReferenceKind.Image,
                        index,
                        imageId));
                }
            }
        }

        if (input.Links is not null)
        {
            for (var index = 0; index < input.Links.Count; index++)
            {
                var linkId = input.Links[index].Id;

                if (linkId is Guid id && project.Links.All(link => link.Id != id))
                {
                    invalidReferences.Add(new InvalidEditProjectReference(
                        EditProjectReferenceKind.Link,
                        index,
                        id));
                }
            }
        }

        return invalidReferences;
    }

    private static bool UpdateProjectStatus(Project project, EditProjectInput input)
    {
        if (input.Status is null)
            return false;

        return project.UpdateStatus(input.Status.Value);
    }

    private static bool UpdateProjectImages(Project project, IReadOnlyList<EditProjectImageInput>? inputs)
    {
        var changed = false;

        foreach (var updateItem in inputs ?? Array.Empty<EditProjectImageInput>())
        {
            var projectImage = project.Images.FirstOrDefault(pi => pi.Id == updateItem.ProjectImageId);

            if (projectImage is null)
                continue;

            changed |= projectImage.UpdateAltText(updateItem.AltText);
            changed |= projectImage.UpdateSortOrder(updateItem.SortOrder);
        }

        return changed;
    }

    private static void AddProjectLinks(Project project, IEnumerable<CreateProjectLinkInput>? inputLinks)
    {
        var linkSortOrder = 0;

        var links = (inputLinks ?? Array.Empty<CreateProjectLinkInput>())
            .Select(l => ProjectLink.Create(
                projectId: project.Id,
                url: l.Url,
                linkText: l.LinkText,
                linkType: l.LinkType,
                sortOrder: linkSortOrder++))
            .ToArray();

        foreach (var link in links)
        {
            project.AddLink(link);
        }
    }

    private static bool UpdateProjectLinks(Project project, IReadOnlyList<EditProjectLinkInput>? inputs)
    {
        if (inputs is null)
            return false;

        var changed = false;

        var existingLinksById = project.Links
            .ToDictionary(link => link.Id);

        var inputIds = inputs
            .Where(input => input.Id is not null)
            .Select(input => input.Id!.Value)
            .ToHashSet();

        var linksToRemove = project.Links
            .Where(link => !inputIds.Contains(link.Id))
            .ToList();

        foreach (var link in linksToRemove)
        {
            project.RemoveLink(link);
            changed = true;
        }

        foreach (var input in inputs)
        {
            if (input.Id is Guid linkId)
            {
                if (!existingLinksById.TryGetValue(linkId, out var existingLink))
                {
                    throw new InvalidOperationException(
                        $"Project link '{linkId}' does not belong to project '{project.Id}'.");
                }

                changed |= existingLink.Update(
                    url: input.Url,
                    linkText: input.LinkText,
                    linkType: input.LinkType);

                changed |= existingLink.UpdateSortOrder(input.SortOrder);

                continue;
            }

            var newLink = ProjectLink.Create(
                projectId: project.Id,
                url: input.Url,
                linkText: input.LinkText,
                linkType: input.LinkType,
                sortOrder: input.SortOrder);

            project.AddLink(newLink);
            changed = true;
        }

        return changed;
    }

    private static bool RemoveProjectLinks(Project project, IReadOnlyList<Guid>? removedLinkIds)
    {
        if (removedLinkIds is null || removedLinkIds.Count == 0)
            return false;

        var changed = false;

        foreach (var linkId in removedLinkIds)
        {
            var link = project.Links.FirstOrDefault(l => l.Id == linkId);

            if (link is null)
                continue;

            project.RemoveLink(link);
            changed = true;
        }

        return changed;
    }

    private static bool NormalizeLinkSortOrder(Project project)
    {
        var changed = false;

        var orderedLinks = project.Links
            .OrderBy(l => l.SortOrder)
            .ToList();

        for (var i = 0; i < orderedLinks.Count; i++)
        {
            changed |= orderedLinks[i].UpdateSortOrder(i);
        }

        return changed;
    }
}
