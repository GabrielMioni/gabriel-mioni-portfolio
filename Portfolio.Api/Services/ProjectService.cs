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
            return await db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> CreateAsync(CreateProjectInput input, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var newProject = new Project
            {
                Title = input.Title,
                Body = input.Body,
                Summary = input.Summary,
                Status = ProjectStatus.Draft,
                PublishedAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.Projects.Add(newProject);
            await db.SaveChangesAsync(ct);

            return newProject;
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

            project.PublishedAt = DateTimeOffset.UtcNow;
            project.UpdatedAt = DateTimeOffset.UtcNow;
            project.Status = ProjectStatus.Published;

            await db.SaveChangesAsync(ct);

            return project;
        }

        public async Task<Project?> ArchiveAsync(Guid id, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            project.PublishedAt = null;
            project.UpdatedAt = DateTimeOffset.UtcNow;
            project.Status = ProjectStatus.Archived;

            await db.SaveChangesAsync(ct);

            return project;
        }
    }
}