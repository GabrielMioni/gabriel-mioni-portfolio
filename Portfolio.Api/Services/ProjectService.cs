using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services
{
    public class ProjectService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ProjectService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> CreateAsync(string title, string? body, string? summary, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var newProject = new Project
            {
                Title = title,
                Body = body,
                Summary = summary,
                Status = ProjectStatus.Draft,
                PublishedAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.Projects.Add(newProject);
            await db.SaveChangesAsync(ct);

            return newProject;
        }

        public async Task<List<Project>> GetPublishedAsync()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            return db.Projects
                .Where(p => p.Status == ProjectStatus.Published)
                .OrderByDescending(p => p.PublishedAt)
                .ToList();
        }
    }
}