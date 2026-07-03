using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Admin;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AdminProjectTagMutation
{
    public Task<ProjectTag> CreateProjectTag(
        CreateProjectTagInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.CreateAsync(input.Name, ct);
    }

    public Task<Project?> UpdateProjectTags(
        UpdateProjectTagsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.UpdateProjectTagsAsync(input.ProjectId, input.TagIds, ct);
    }
}
