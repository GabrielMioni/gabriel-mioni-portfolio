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

        public async Task<List<Project>> GetProjectsAsync(int skip = 0, int take = 10)
        {
            return await _context.Projects.Skip(skip).Take(take).ToListAsync();
        }

        public async Task<Project> AddProjectAsync(Project newProject)
        {
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            return newProject;
        }

        public async Task<int> GetProjectCountAsync()
        {
            return await _context.Projects.CountAsync();
        }
    }
}
