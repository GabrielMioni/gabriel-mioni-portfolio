using backend.Features.Projects.Models;
using backend.Features.Projects.Repositories;

namespace backend.Features.Projects.GraphQL
{
    public class ProjectResolver : IProjectResolver
    {
        private readonly IProjectsRepository _projectsRepository;

        public ProjectResolver(IProjectsRepository projectsRepository)
        {
            _projectsRepository = projectsRepository;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _projectsRepository.GetAllProjects();
        }

        public async Task<Project> AddProjectAsync(Project newProject)
        {
            return await _projectsRepository.AddProject(newProject);
        }
    }
}
