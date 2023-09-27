using backend.Features.Projects.Models;
using backend.Features.Projects.Repositories;

namespace backend.GraphQL
{
    public class RootQuery
    {
        [GraphQLDescription("A simple hello world query")]
        public string HelloWorld() => "Hello, world!";

        [GraphQLDescription("Retrieve the list of projects")]
        public async Task<List<Project>> GetProjectsAsync([Service]IProjectsRepository repository)
        {
            return await repository.GetAllProjects();
        }
    }
}
