using backend.Features.Projects.Models;

namespace backend.Features.Projects.GraphQL
{
    public interface IProjectResolver
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project> AddProjectAsync(Project newProject);
    }
}
