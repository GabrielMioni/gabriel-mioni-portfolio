using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects
{

    public class ProjectQuery
    {
        public Task<Project?> GetProjectById(Guid id, [Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.GetByIdAsync(id, ct);
        }

        public Task<List<Project>> GetPublsihedProjects([Service] ProjectService projects, CancellationToken ct = default)
        {
            return projects.GetPublishedAsync(ct);
        }

        [UsePaging]
        [UseSorting]
        [UseFiltering]
        public IQueryable<Project> GetProjects(
            [Service] AppDbContext db,
            [Service] ProjectService projectService,
            bool includeUnpublished = false)
        {
            return projectService.QueryProjects(db, includeUnpublished);
        }
    }
}
