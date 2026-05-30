using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Admin
{
    public class AdminProjectMutation
    {
        public Task<Project> CreateProject(
            CreateProjectInput input,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            return projects.CreateAsync(input, ct);
        }

        public async Task<DeleteProjectPayload> DeleteProject(
            DeleteProjectInput input,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            var projectId = await projects.DeleteProjectAsync(input.ProjectId, ct);

            return new DeleteProjectPayload(projectId);
        }
        public Task<Project?> EditProject(
            EditProjectInput input,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            return projects.EditProjectAsync(input, ct);
        }

        public Task<Project?> PublishProject(
            Guid id,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            return projects.PublishAsync(id, ct);
        }

        public Task<Project?> ArchiveProject(
            Guid id,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            return projects.ArchiveAsync(id, ct);
        }       
    }
}
