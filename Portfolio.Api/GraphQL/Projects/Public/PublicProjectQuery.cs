using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.GraphQL.Projects.Public.Payloads;
using Portfolio.Api.Services;

namespace Portfolio.Api.GraphQL.Projects.Public
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PublicProjectQuery
    {
        public async Task<List<PublicProjectTagDto>> GetPublishedTags(
            [Service] AppDbContext db,
            [Service] ProjectService projectService,
            CancellationToken ct)
        {
            var publishedIds = projectService
                .QueryProjects(db, includeUnpublished: false)
                .Select(p => p.Id);

            return await db.Tags
                .Where(t => t.Projects.Any(p => publishedIds.Contains(p.Id)))
                .OrderBy(t => t.Name)
                .Select(t => new PublicProjectTagDto { Id = t.Id, Name = t.Name, Value = t.Value })
                .ToListAsync(ct);
        }

        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public IQueryable<PublicProjectDto> GetPublishedProjects(
            string[]? tagValues,
            [Service] AppDbContext db,
            [Service] ProjectService projectService)
        {
            var query = projectService
                .QueryProjects(db, includeUnpublished: false)
                .AsNoTracking();

            if (tagValues is { Length: > 0 })
                query = query.Where(p => p.Tags.Any(t => tagValues.Contains(t.Value)));

            return query.Select(project => new PublicProjectDto
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