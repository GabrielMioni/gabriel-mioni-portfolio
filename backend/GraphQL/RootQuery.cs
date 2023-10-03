using backend.Features.Projects.GraphQL;
// using backend.Features.Projects.Models;
using backend.Features.Projects.Repositories;

namespace backend.GraphQL
{
    public class RootQuery
    {
        [GraphQLDescription("A simple hello world query")]
        public string HelloWorld() => "Hello, world!";

        [GraphQLDescription("Retrieve the list of projects")]
        public async Task<ProjectResult> GetProjectListAsync([Service]IProjectsRepository repository)
        {
            var result = new ProjectResult();
            result.Errors = new List<string>();
            try
            {
                var projects = await repository.GetAllProjectsAsync();
                result.Projects = projects;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result.Errors = new List<string> { "An error occurred while fetching projects." };
            }
            return result;
        }
    }
}
