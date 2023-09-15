using backend.Models;

namespace backend.GraphQL
{
    public class RootQuery
    {
        [GraphQLDescription("A simple hello world query")]
        public string HelloWorld() => "Hello, world!";

        [GraphQLDescription("Retrieve the list of projects")]
        public List<Project> GetProjects()
        {
            return new List<Project>
            {
                new Project { Id = 1, Name = "React Checkers", Description = "This 2-player checkers game...", Image = "reactCheckers", Git = "https://github.com/GabrielMioni/react-checkers" },
                new Project { Id = 2, Name = "Project B", Description = "This is a description for Project B.", Image = null, Git = null },
                new Project { Id = 3, Name = "Project C", Description = "This is a description for Project C.", Image = null, Git = null }
            };
        }
    }
}
