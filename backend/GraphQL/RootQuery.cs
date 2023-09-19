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
                new Project { Id = 1, Name = "React Checkers", Description = "This 2-player checkers game, built in React.js, offers a mobile-friendly design and a board that maintains a state history, allowing players to undo moves for a seamless gaming experience. The rules deviate slightly from traditional tournament checkers; you aren't obligated to make jumps, giving you more strategic freedom.\n\nThe game's logic resides primarily in the ReactCheckers component, where the board and its pieces are represented as objects. As players make their moves, the board state is recorded, making it easy to undo actions. Victory conditions are checked at the end of each turn, giving you a game that is as challenging as it is enjoyable.", Image = "reactCheckers", Git = "https://github.com/GabrielMioni/react-checkers" },
                new Project { Id = 2, Name = "Project B", Description = "This is a description for Project B.", Image = null, Git = null },
                new Project { Id = 3, Name = "Project C", Description = "This is a description for Project C.", Image = null, Git = null }
            };
        }
    }
}
