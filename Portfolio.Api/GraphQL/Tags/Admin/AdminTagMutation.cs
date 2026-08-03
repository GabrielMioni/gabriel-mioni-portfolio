using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Tags.Admin.Inputs;
using Portfolio.Api.GraphQL.Tags.Admin.Payloads;
using Portfolio.Api.Services.Tags;
using Portfolio.Api.Services.Tags.Results;

namespace Portfolio.Api.GraphQL.Tags.Admin;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AdminTagMutation
{
    public async Task<CreateProjectTagsPayload> CreateProjectTags(
        CreateProjectTagsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        var userErrors = TagInputValidator.ValidateNames(input.Names);

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

    public async Task<UpdateProjectTagsPayload> UpdateProjectTags(
        UpdateProjectTagsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        var userErrors = TagInputValidator.ValidateProjectTagIds(input.TagIds);

        if (userErrors.Count > 0)
        {
            return new UpdateProjectTagsPayload(
                Project: null,
                UserErrors: userErrors);
        }

        var result = await tags.UpdateProjectTagsAsync(
            input.ProjectId,
            input.TagIds,
            ct);

        if (result.ProjectWasNotFound)
        {
            return new UpdateProjectTagsPayload(
                Project: null,
                UserErrors:
                [
                    UserError.NotFound(
                        $"Project '{input.ProjectId}' was not found.",
                        "input", "projectId")
                ]);
        }

        if (result.InvalidReferences.Count > 0)
        {
            return new UpdateProjectTagsPayload(
                Project: null,
                UserErrors: result.InvalidReferences
                    .Select(reference => UserError.InvalidReference(
                        $"Project tag '{reference.Id}' was not found.",
                        "input", "tagIds", reference.InputIndex.ToString()))
                    .ToArray());
        }

        if (result.Project is null)
            throw new InvalidOperationException("A successful tag update returned no project.");

        return new UpdateProjectTagsPayload(
            Project: result.Project,
            UserErrors: []);
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
        var userErrors = TagInputValidator.ValidateName(input.Name);

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

    public async Task<RemoveTagFromProjectsPayload> RemoveTagFromProjects(
        RemoveTagFromProjectsInput input,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        var result = await tags.RemoveTagFromProjectsAsync(
            input.TagId,
            input.ProjectIds,
            ct);

        if (result.TagWasNotFound)
        {
            return new RemoveTagFromProjectsPayload(
                ProjectIds: null,
                UserErrors:
                [
                    UserError.NotFound(
                        $"Project tag '{input.TagId}' was not found.",
                        "input", "tagId")
                ]);
        }

        if (result.InvalidReferences.Count > 0)
        {
            return new RemoveTagFromProjectsPayload(
                ProjectIds: null,
                UserErrors: result.InvalidReferences
                    .Select(reference => UserError.InvalidReference(
                        $"Project '{reference.Id}' was not found.",
                        "input", "projectIds", reference.InputIndex.ToString()))
                    .ToArray());
        }

        if (result.ProjectIds is null)
            throw new InvalidOperationException("Successful tag removal returned no project IDs.");

        return new RemoveTagFromProjectsPayload(
            ProjectIds: result.ProjectIds,
            UserErrors: []);
    }
}
