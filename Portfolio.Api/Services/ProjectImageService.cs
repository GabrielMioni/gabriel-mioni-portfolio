using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Types;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.Services;

public class ProjectImageService
{
    private readonly IObjectStorage _storage;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProjectImageService(IObjectStorage storage, IDbContextFactory<AppDbContext> dbFactory)
    {
        _storage = storage;
        _dbFactory = dbFactory;
    }

    private static string ExtFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => "jpg",
        "image/png" => "png",
        "image/webp" => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(contentType), $"Unsupported content type: {contentType}")
    };

    public async Task<IReadOnlyList<ProjectImageUploadInstruction>> PrepareImageUploadAsync(
        RequestProjectImageUploadsInput input,
        CancellationToken ct
    ) {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var projectId = input.ProjectId;

        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project == null)
            throw new InvalidOperationException("Project not found");

        var instructions = new List<ProjectImageUploadInstruction>(input.Items.Count);

        var nextSortOrder = project.Images.Count == 0
            ? 0
            : project.Images.Max(x => x.SortOrder) + 1;

        foreach (var item in input.Items)
        {
            var imageGuid = Guid.NewGuid();

            var fullKey = $"projects/{projectId}/{imageGuid:N}_full.{ExtFor(item.FullContentType)}";
            var thumbKey = $"projects/{projectId}/{imageGuid:N}_thumb.{ExtFor(item.ThumbContentType)}";

            var projectImage = ProjectImage.CreatePending(
                projectId,
                fullKey,
                thumbKey,
                nextSortOrder++
            );

            db.Set<ProjectImage>().Add(projectImage);

            var thumbProjectImageUploadTarget = new ProjectImageUploadTarget(
                thumbKey,
                _storage.CreatePresignedPutUrl(thumbKey, item.ThumbContentType, TimeSpan.FromMinutes(5)),
                _storage.GetPublicUrl(thumbKey),
                item.ThumbContentType
            );

            var fullProjectImageUploadTarget = new ProjectImageUploadTarget(
                fullKey,
                _storage.CreatePresignedPutUrl(fullKey, item.FullContentType, TimeSpan.FromMinutes(5)),
                _storage.GetPublicUrl(fullKey),
                item.FullContentType
            );

            instructions.Add(new ProjectImageUploadInstruction(
                item.ClientId,
                fullProjectImageUploadTarget,
                thumbProjectImageUploadTarget
            ));
        }

        await db.SaveChangesAsync(ct);

        return instructions;
    }
}