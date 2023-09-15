using backend.Models;
using HotChocolate;
using System.Collections.Generic;
using System.Linq;

namespace backend.GraphQL.Resolvers
{
    public class ProjectResolver
    {
        private readonly List<Project> _projects;

        public ProjectResolver()
        {
            _projects = new List<Project>
            {
                new Project { Id = 1, Name = "React Checkers", Description = "This 2-player checkers game...", Image = "reactCheckers", Git = "https://github.com/GabrielMioni/react-checkers" },
                new Project { Id = 2, Name = "Project B", Description = "This is a description for Project B.", Image = null, Git = null },
                new Project { Id = 3, Name = "Project C", Description = "This is a description for Project C.", Image = null, Git = null }
            };
        }

        public Project GetProjectById(int id)
        {
            return _projects.FirstOrDefault(project => project.Id == id);
        }

        public IEnumerable<Project> GetAllProjects()
        {
            return _projects;
        }
    }
}
