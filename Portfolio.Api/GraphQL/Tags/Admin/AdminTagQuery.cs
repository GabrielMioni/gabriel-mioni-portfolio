using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Tags.Admin.Payloads;
using Portfolio.Api.Services.Tags;

namespace Portfolio.Api.GraphQL.Tags.Admin;

[ExtendObjectType(OperationTypeNames.Query)]
public class AdminTagQuery
{
    public Task<List<ProjectTag>> GetTags(
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.GetAllAsync(ct);
    }

    public Task<List<Project>> GetProjectsByTagId(
        Guid tagId,
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.GetProjectsByTagIdAsync(tagId, ct);
    }

    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseSorting]
    [UseFiltering]
    public Task<IEnumerable<ProjectTagSummary>> GetTagSummaries(
        bool showOrphaned,
        [Service] ProjectTagService tags,
        CancellationToken ct)
    {
        return tags.GetSummariesAsync(showOrphaned, ct);
    }
}
