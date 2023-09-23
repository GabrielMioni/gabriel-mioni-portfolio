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

        public async Task<List<Project>> GetAllProjects()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<Project> AddProject(Project newProject)
        {
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            return newProject;
        }
    }
}
