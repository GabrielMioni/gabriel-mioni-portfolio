using backend.Features.Projects.Models;
using backend.Features.Projects.Repositories;

namespace backend.GraphQL
{
    public class RootMutation
    {
        public async Task<Project> AddProjectAsync(string name, string description, string? git, [Service] IProjectsRepository repository)
        {
            Project newProject = new Project {
                Name = name,
                Description = description,
                Git = git,
            };

            return await repository.AddProjectAsync(newProject);
        }

        public async Task<OperationResult> RemoveProjectAsync(string id, [Service] IProjectsRepository repository)
        {
            return await repository.RemoveProjectAsync(id);
        }
    }
}
