using Portfolio.Api.Domain.Projects;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Admin;

[ExtendObjectType(OperationTypeNames.Query)]
public class AdminProjectTagQuery
{
    public Task<List<ProjectTag>> GetTags(
        [Service] ProjectTagService tags,
        CancellationToken ct = default)
    {
        return tags.GetAllAsync(ct);
    }
}
