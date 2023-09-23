using backend.Features.Projects.Models;

namespace backend.Features.Projects.Repositories
{
    public interface IProjectsRepository
    {
        Task<List<Project>> GetAllProjects();
        Task<Project> AddProject(Project newProject);
    }
}
