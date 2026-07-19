using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Results;

namespace Portfolio.Api.GraphQL.Projects.Admin;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AdminProjectTagMutation
{
    public async Task<CreateProjectTagsPayload> CreateProjectTags(
        CreateProjectTagsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        var userErrors = ProjectTagInputValidator.ValidateNames(input.Names);

        if (userErrors.Count > 0)
        {
            return new CreateProjectTagsPayload(
                Tags: null,
                UserErrors: userErrors);
        }

        var result = await tags.CreateManyAsync(input.Names, ct);

        if (result.Conflicts.Count > 0)
        {
            return new CreateProjectTagsPayload(
                Tags: null,
                UserErrors: result.Conflicts
                    .Select(conflict => UserError.Conflict(
                        $"A tag with the name '{conflict.Name.Trim()}' already exists.",
                        "input", "names", conflict.InputIndex.ToString()))
                    .ToArray());
        }

        if (result.Tags is null)
            throw new InvalidOperationException("Successful tag creation returned no tags.");

        return new CreateProjectTagsPayload(
            Tags: result.Tags,
            UserErrors: []);
    }

    public Task<Project?> UpdateProjectTags(
        UpdateProjectTagsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.UpdateProjectTagsAsync(input.ProjectId, input.TagIds, ct);
    }

    public async Task<DeleteProjectTagPayload> DeleteProjectTag(
        DeleteProjectTagInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        var deletedTagId = await tags.DeleteAsync(input.Id, ct);

        if (deletedTagId is null)
        {
            return new DeleteProjectTagPayload(
                DeletedTagId: null,
                UserErrors:
                [
                    UserError.NotFound(
                        $"Project tag '{input.Id}' was not found.",
                        "input", "id")
                ]);
        }

        return new DeleteProjectTagPayload(
            DeletedTagId: deletedTagId,
            UserErrors: []);
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
