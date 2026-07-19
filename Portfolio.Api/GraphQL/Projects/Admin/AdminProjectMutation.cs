using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Results;

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
        public async Task<EditProjectPayload> EditProject(
            EditProjectInput input,
            [Service] ProjectService projects,
            CancellationToken ct = default)
        {
            var userErrors = ProjectInputValidator.ValidateEdit(input);

            if (userErrors.Count > 0)
            {
                return new EditProjectPayload(
                    Project: null,
                    UserErrors: userErrors);
            }

            var result = await projects.EditProjectAsync(input, ct);

            if (result.ProjectWasNotFound)
            {
                return new EditProjectPayload(
                    Project: null,
                    UserErrors:
                    [
                        new UserError(
                            UserErrorCode.NotFound,
                            $"Project '{input.Id}' was not found.",
                            ["input", "id"])
                    ]);
            }

            if (result.InvalidReferences.Count > 0)
            {
                return new EditProjectPayload(
                    Project: null,
                    UserErrors: result.InvalidReferences
                        .Select(reference => ToUserError(input.Id, reference))
                        .ToArray());
            }

            if (result.Project is null)
                throw new InvalidOperationException("A successful project edit returned no project.");

            return new EditProjectPayload(
                Project: result.Project,
                UserErrors: []);
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

        private static UserError ToUserError(
            Guid projectId,
            InvalidEditProjectReference reference)
        {
            return reference.Kind switch
            {
                EditProjectReferenceKind.Image => new UserError(
                    UserErrorCode.InvalidReference,
                    $"Project image '{reference.Id}' does not belong to project '{projectId}'.",
                    ["input", "images", reference.InputIndex.ToString(), "projectImageId"]),
                EditProjectReferenceKind.Link => new UserError(
                    UserErrorCode.InvalidReference,
                    $"Project link '{reference.Id}' does not belong to project '{projectId}'.",
                    ["input", "links", reference.InputIndex.ToString(), "id"]),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(reference),
                    reference.Kind,
                    "Unknown project reference kind.")
            };
        }
    }
}
