using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Results;

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

    public async Task<RenameProjectTagPayload> RenameProjectTag(
        RenameProjectTagInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        var userErrors = ProjectTagInputValidator.ValidateName(input.Name);

        if (userErrors.Count > 0)
        {
            return new RenameProjectTagPayload(
                Tag: null,
                UserErrors: userErrors);
        }

        var result = await tags.RenameAsync(input.Id, input.Name, ct);

        if (result.Outcome == RenameProjectTagOutcome.NotFound)
        {
            return new RenameProjectTagPayload(
                Tag: null,
                UserErrors:
                [
                    UserError.NotFound(
                        $"Project tag '{input.Id}' was not found.",
                        "input", "id")
                ]);
        }

        if (result.Outcome == RenameProjectTagOutcome.Conflict)
        {
            return new RenameProjectTagPayload(
                Tag: null,
                UserErrors:
                [
                    UserError.Conflict(
                        $"A tag with the name '{input.Name.Trim()}' already exists.",
                        "input", "name")
                ]);
        }

        if (result.Tag is null)
            throw new InvalidOperationException("A successful tag rename returned no tag.");

        return new RenameProjectTagPayload(
            Tag: result.Tag,
            UserErrors: []);
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
