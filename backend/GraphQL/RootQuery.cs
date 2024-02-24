using backend.Features.Projects.GraphQL;
using backend.Features.Projects.Repositories;

namespace backend.GraphQL
{
    public class RootQuery
    {
        [GraphQLDescription("A simple hello world query")]
        public string HelloWorld() => "Hello, world!";

        [GraphQLDescription("Retrieve the list of projects")]
        public async Task<ProjectResult> GetProjectsAsync(
            [Service] IProjectsRepository repository,
            int skip = 0,
            int take = 10
        )
        {
            var result = new ProjectResult();
            result.Errors = new List<string>();
            try
            {
                var projects = await repository.GetProjectsAsync(skip, take, true);
                result.Nodes = projects;
                result.Count = await repository.GetProjectCountAsync();
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
