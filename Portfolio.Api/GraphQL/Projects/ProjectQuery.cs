using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects
{
    public class ProjectQuery
    {
        public Project GetProject()
        {
            return new Project
            {
                Id = Guid.NewGuid(),
                Title = "Project 1",
                Summary = "This is a summary of project 1.",
                Body = "This is the body of project 1.",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Status = ProjectStatus.Published
            };
        }
    }
}
