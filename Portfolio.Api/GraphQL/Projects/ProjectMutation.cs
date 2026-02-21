using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects
{
    public class ProjectMutation
    {
        public Task<Project> CreateProject(CreateProjectInput input, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.CreateAsync(input, ct);
        }

        public Task<Project?> PublishProject(Guid id, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.PublishAsync(id, ct);
        }

        public Task<Project?> ArchiveProject(Guid id, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.ArchiveAsync(id, ct);
        }
    }
}
