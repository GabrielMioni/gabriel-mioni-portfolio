using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Types;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.GraphQL.Projects
{
    public class ProjectMutation
    {
        public Task<Project> CreateProject(CreateProjectInput input, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.CreateAsync(input, ct);
        }

        public Task<Project?> EditProject(EditProjectInput input, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.EditProjectAsync(input, ct);
        }

        public Task<Project?> PublishProject(Guid id, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.PublishAsync(id, ct);
        }

        public Task<Project?> ArchiveProject(Guid id, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.ArchiveAsync(id, ct);
        }

        public RequestUploadPayload RequestTestUpload(RequestUploadInput input, [Service] IR2Storage storage)
        {
            var key = $"test/{Guid.NewGuid():N}.webp";

            var uploadUrl = storage.CreatePresignedPutUrl(
                key,
                input.ContentType,
                TimeSpan.FromMinutes(5));

            var publicUrl = storage.GetPublicUrl(key);

            return new RequestUploadPayload(
                key,
                uploadUrl,
                publicUrl);
        }
    }
}
