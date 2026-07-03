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
                    PublishedAt = project.PublishedAt,
                    Images = project.Images
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new PublicProjectImageDto
                        {
                            Id = i.Id,
                            FullKey = i.FullKey,
                            ThumbKey = i.ThumbKey,
                            AltText = i.AltText,
                            SortOrder = i.SortOrder
                        })
                        .ToList(),
                    Links = project.Links
                        .OrderBy(l => l.SortOrder)
                        .Select(l => new PublicProjectLinkDto
                        {
                            Id = l.Id,
                            Url = l.Url,
                            LinkText = l.LinkText,
                            LinkType = l.LinkType,
                            SortOrder = l.SortOrder
                        })
                        .ToList(),
                    Tags = project.Tags
                        .OrderBy(t => t.Name)
                        .Select(t => new PublicProjectTagDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Value = t.Value
                        })
                        .ToList()
                });
        }
    }
}