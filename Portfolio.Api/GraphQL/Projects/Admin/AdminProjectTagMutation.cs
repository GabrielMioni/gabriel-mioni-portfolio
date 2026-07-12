using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Admin;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AdminProjectTagMutation
{
    public Task<List<ProjectTag>> CreateProjectTags(
        CreateProjectTagsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.CreateManyAsync(input.Names, ct);
    }

    public Task<Project?> UpdateProjectTags(
        UpdateProjectTagsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.UpdateProjectTagsAsync(input.ProjectId, input.TagIds, ct);
    }

    public Task<Guid?> DeleteProjectTag(
        DeleteProjectTagInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.DeleteAsync(input.Id, ct);
    }

    public Task<ProjectTag?> RenameProjectTag(
        RenameProjectTagInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.RenameAsync(input.Id, input.Name, ct);
    }

    public async Task<IReadOnlyList<Guid>> RemoveTagFromProjects(
        RemoveTagFromProjectsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        await tags.RemoveTagFromProjectsAsync(input.TagId, input.ProjectIds, ct);
        return input.ProjectIds;
    }
}
