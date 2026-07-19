using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Admin
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class AdminProjectMutation
    {
        public async Task<CreateProjectPayload> CreateProject(
            CreateProjectInput input,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            var userErrors = ProjectInputValidator.ValidateCreate(input);

            if (userErrors.Count > 0)
            {
                return new CreateProjectPayload(
                    Project: null,
                    UserErrors: userErrors);
            }

            var project = await projects.CreateAsync(input, ct);

            return new CreateProjectPayload(
                Project: project,
                UserErrors: []);
        }

        public async Task<DeleteProjectPayload> DeleteProject(
            DeleteProjectInput input,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            var projectId = await projects.DeleteProjectAsync(input.ProjectId, ct);

            if (projectId is null)
            {
                return new DeleteProjectPayload(
                    DeletedProjectId: null,
                    UserErrors:
                    [
                        new UserError(
                            UserErrorCode.NotFound,
                            $"Project '{input.ProjectId}' was not found.")
                    ]);
            }

            return new DeleteProjectPayload(
                DeletedProjectId: projectId,
                UserErrors: []);
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
