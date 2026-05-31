using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Admin
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AdminProjectQuery
    {
        public Task<Project?> GetProjectById(Guid id, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.GetByIdAsync(id, ct);
        }

        public Task<List<Project>> GetPublsihedProjects([Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.GetPublishedAsync(ct);
        }

        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseSorting]
        [UseFiltering]
        public IQueryable<Project> GetProjects(
            bool includeUnpublished,
            [Service] AppDbContext db,
            [Service] ProjectService projectService)
        {
            return projectService.QueryProjects(db, includeUnpublished);
        }
    }
}
