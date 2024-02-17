using backend.Data;
using backend.Features.Projects.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Projects.Repositories
{
    public class ProjectsRepository : IProjectsRepository
    {
        private readonly ApplicationDbContext _context;
        public ProjectsRepository(ApplicationDbContext context) { 
            _context = context;
        }

        public async Task<List<Project>> GetProjectsAsync(int skip = 0, int take = 10, bool includeInactive = false)
        {
            var query = _context.Projects.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(p => p.Active);
            }

            return await query.Skip(skip).Take(take).ToListAsync();
        }

        public async Task<Project> AddProjectAsync(Project newProject)
        {
            newProject.Id = Guid.NewGuid().ToString();
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            return newProject;
        }

        public async Task<OperationResult> RemoveProjectAsync(string id)
        {
            var project = _context.Projects.Where(p => p.Id  == id).FirstOrDefault();

            var result = new OperationResult();

            if (project == null)
            {
                return OperationResult.Fail($"Unable to find project with id {id} of ");
            }

            project.Active = false;
            await _context.SaveChangesAsync();

            return OperationResult.Success($"Project with id {id} has been successfully deactivated.");
        }

        public async Task<int> GetProjectCountAsync()
        {
            return await _context.Projects.CountAsync();
        }
    }
}
