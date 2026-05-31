using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.GraphQL.Projects.Public.Payloads;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Public
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PublicProjectQuery
    {
        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public IQueryable<PublicProjectDto> GetPublishedProjects(
            [Service] AppDbContext db,
            [Service] ProjectService projectService)
        {
            return projectService
                .QueryProjects(db, includeUnpublished: false)
                .AsNoTracking()
                .Select(project => new PublicProjectDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    Summary = project.Summary,
                    Body = project.Body,
                    PublishedAt = project.PublishedAt
                });
        }
    }
}