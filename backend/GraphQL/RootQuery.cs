using backend.Features.Projects.GraphQL;
using backend.Features.Projects.Models;

namespace backend.GraphQL
{
    public class RootQuery
    {
        private readonly IProjectResolver _projectResolver;

        public RootQuery(IProjectResolver projectResolver)
        {
            _projectResolver = projectResolver;
        }

        [GraphQLDescription("A simple hello world query")]
        public string HelloWorld() => "Hello, world!";

        [GraphQLDescription("Retrieve the list of projects")]
        public async Task<List<Project>> GetProjects()
        {
            return (await _projectResolver.GetAllProjectsAsync()).ToList();
        }
    }
}
