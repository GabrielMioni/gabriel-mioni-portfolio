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
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> CreateAsync(CreateProjectInput input, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var newProject = Project.Create(
                title: input.Title,
                summary: input.Summary,
                body: input.Body,
                status: ProjectStatus.Draft);

            db.Projects.Add(newProject);
            await db.SaveChangesAsync(ct);

            return newProject;
        }

        public async Task<Project?> EditProjectAsync(EditProjectInput input, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var project = await db.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == input.Id, ct);

            if (project is null) return null;

            var changed = false;

            changed |= project.UpdateDetails(
                title: input.Title ?? project.Title,
                summary: input.Summary ?? project.Summary,
                body: input.Body ?? project.Body);

            if (input.Status is not null)
            {
                changed |= project.UpdateStatus(input.Status.Value);
            }

            var projectImageUpdates = input.Images ?? Array.Empty<EditProjectImageInput>();

            foreach (var updateItem in projectImageUpdates)
            {
                var projectImage = project.Images.FirstOrDefault(pi => pi.Id == updateItem.ProjectImageId);

                if (projectImage is null) continue;

                changed |= projectImage.UpdateAltText(updateItem.AltText);
                changed |= projectImage.UpdateSortOrder(updateItem.SortOrder);
            }

            if (!changed)
                return project;

            await db.SaveChangesAsync(ct);
            return project;
        }

        public async Task<List<Project>> GetPublishedAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            return await db.Projects
                .Where(p => p.Status == ProjectStatus.Published)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync(ct);
        }

        public IQueryable<Project> QueryProjects(AppDbContext db, bool includeUnpublished)
        {
            var q = db.Projects.AsQueryable();

            if (!includeUnpublished)
                q = q.Where(p => p.Status == ProjectStatus.Published);

            return q;
        }          

        public async Task<Project?> PublishAsync(Guid id, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            project.UpdateStatus(ProjectStatus.Published);

            await db.SaveChangesAsync(ct);

            return project;
        }

        public async Task<Project?> ArchiveAsync(Guid id, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            project.UpdateStatus(ProjectStatus.Archived);

            await db.SaveChangesAsync(ct);

            return project;
        }
    }
}