using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;

namespace Portfolio.Api.GraphQL.Projects.Admin;

internal static class ProjectImageInputValidator
{
    private static readonly HashSet<string> SupportedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public static IReadOnlyList<UserError> ValidatePrepare(
        PrepareProjectImageUploadsInput input)
    {
        var userErrors = new List<UserError>();
        var seenClientIds = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < input.Items.Count; index++)
        {
            var item = input.Items[index];
            var fieldPrefix = new[] { "input", "items", index.ToString() };

            ValidateClientId(item.ClientId, fieldPrefix, seenClientIds, userErrors);
            ValidateContentType(item.FullContentType, "fullContentType", fieldPrefix, userErrors);
            ValidateContentType(item.ThumbContentType, "thumbContentType", fieldPrefix, userErrors);
            ValidatePositive(item.FullSizeBytes, "fullSizeBytes", fieldPrefix, userErrors);
            ValidatePositive(item.ThumbSizeBytes, "thumbSizeBytes", fieldPrefix, userErrors);
            ValidatePositive(item.Width, "width", fieldPrefix, userErrors);
            ValidatePositive(item.Height, "height", fieldPrefix, userErrors);
            ValidateMaximum(
                item.FullSizeBytes,
                ProjectImage.MaxFullSizeBytes,
                "fullSizeBytes",
                "Full-size image cannot exceed 15 MiB.",
                fieldPrefix,
                userErrors);
            ValidateMaximum(
                item.ThumbSizeBytes,
                ProjectImage.MaxThumbnailSizeBytes,
                "thumbSizeBytes",
                "Thumbnail cannot exceed 3 MiB.",
                fieldPrefix,
                userErrors);
            ValidateMaximum(
                item.Width,
                ProjectImage.MaxDimensionPixels,
                "width",
                $"Image width cannot exceed {ProjectImage.MaxDimensionPixels} pixels.",
                fieldPrefix,
                userErrors);
            ValidateMaximum(
                item.Height,
                ProjectImage.MaxDimensionPixels,
                "height",
                $"Image height cannot exceed {ProjectImage.MaxDimensionPixels} pixels.",
                fieldPrefix,
                userErrors);
        }

        return userErrors;
    }

    private static void ValidateClientId(
        string clientId,
        IReadOnlyList<string> fieldPrefix,
        ISet<string> seenClientIds,
        ICollection<UserError> userErrors)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            userErrors.Add(UserError.Validation(
                "Client ID is required.",
                [.. fieldPrefix, "clientId"]));
            return;
        }

        var normalizedClientId = clientId.Trim();

        if (normalizedClientId.Length > ProjectImage.MaxClientIdLength)
        {
            userErrors.Add(UserError.Validation(
                $"Client ID cannot exceed {ProjectImage.MaxClientIdLength} characters.",
                [.. fieldPrefix, "clientId"]));
        }

        if (!seenClientIds.Add(normalizedClientId))
        {
            userErrors.Add(UserError.Conflict(
                $"Client ID '{normalizedClientId}' duplicates another requested image.",
                [.. fieldPrefix, "clientId"]));
        }
    }

    private static void ValidateContentType(
        string contentType,
        string fieldName,
        IReadOnlyList<string> fieldPrefix,
        ICollection<UserError> userErrors)
    {
        if (!SupportedContentTypes.Contains(contentType))
        {
            userErrors.Add(UserError.Validation(
                "Content type must be image/jpeg, image/png, or image/webp.",
                [.. fieldPrefix, fieldName]));
        }
    }

    private static void ValidatePositive(
        int value,
        string fieldName,
        IReadOnlyList<string> fieldPrefix,
        ICollection<UserError> userErrors)
    {
        if (value <= 0)
        {
            userErrors.Add(UserError.Validation(
                "Value must be greater than zero.",
                [.. fieldPrefix, fieldName]));
        }
    }

    private static void ValidateMaximum(
        int value,
        int maximum,
        string fieldName,
        string message,
        IReadOnlyList<string> fieldPrefix,
        ICollection<UserError> userErrors)
    {
        if (value > maximum)
        {
            userErrors.Add(UserError.Validation(
                message,
                [.. fieldPrefix, fieldName]));
        }
    }
}
