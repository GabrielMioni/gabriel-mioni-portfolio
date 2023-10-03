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

        public async Task<IEnumerable<Models.Project>> GetAllProjectsAsync()
        {
            return await _projectsRepository.GetAllProjectsAsync();
        }

        public async Task<Models.Project> AddProjectAsync(Models.Project newProject)
        {
            return await _projectsRepository.AddProjectAsync(newProject);
        }

        Task<IEnumerable<Project>> IProjectResolver.GetAllProjectsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Project> AddProjectAsync(Project newProject)
        {
            throw new NotImplementedException();
        }
    }
}
