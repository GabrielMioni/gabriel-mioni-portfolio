using backend.Features.Projects.Models;
using backend.Features.Projects.Repositories;

namespace backend.GraphQL
{
    public class RootMutation
    {
        public async Task<Project> AddProjectAsync(
            ProjectInput input,
            [Service] IProjectsRepository repository)
        {
            Project newProject = new Project {
                Name = input.Name,
                Description = input.Description,
                Git = input.Git,
            };

            return await repository.AddProjectAsync(newProject);
        }
    }
}
