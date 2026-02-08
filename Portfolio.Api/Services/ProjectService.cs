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
    }
}