using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Inputs;

namespace Portfolio.Api.Services
{
    public class ProjectService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ProjectService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            return await db.Projects
                .Include(p => p.Images)
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<Project> CreateAsync(CreateProjectInput input, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var newProject = Project.Create(
                title: input.Title,
                summary: input.Summary,
                body: input.Body,
                status: ProjectStatus.Draft);

            db.Projects.Add(newProject);

            AddProjectLinks(newProject, input.Links);

            await db.SaveChangesAsync(ct);

            return newProject;
        }

        public async Task<Project?> EditProjectAsync(EditProjectInput input, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var project = await db.Projects
                .Include(p => p.Images)
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == input.Id, ct);

            if (project is null)
                return null;

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
                return project;

            await db.SaveChangesAsync(ct);

            return project;
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
                    link: l.Link,
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
            var changed = false;

            foreach (var updateItem in inputs ?? Array.Empty<EditProjectLinkInput>())
            {
                if (updateItem.Id is Guid linkId)
                {
                    var existingLink = project.Links.FirstOrDefault(l => l.Id == linkId);

                    if (existingLink is null)
                        continue;

                    changed |= existingLink.Update(
                        link: updateItem.Link,
                        linkText: updateItem.LinkText,
                        linkType: updateItem.LinkType);

                    changed |= existingLink.UpdateSortOrder(updateItem.SortOrder);

                    continue;
                }

                var newLink = ProjectLink.Create(
                    projectId: project.Id,
                    link: updateItem.Link,
                    linkText: updateItem.LinkText,
                    linkType: updateItem.LinkType,
                    sortOrder: updateItem.SortOrder);

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
}