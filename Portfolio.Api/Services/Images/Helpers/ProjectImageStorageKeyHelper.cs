using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Services.Images.Helpers;

internal static class ProjectImageStorageKeyHelper
{
    public static HashSet<string> GetStorageKeys(IEnumerable<ProjectImage> images)
    {
        return images
            .SelectMany(image => new[] { image.FullKey, image.ThumbKey })
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToHashSet();
    }
}
