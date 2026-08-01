using Xunit;

namespace Portfolio.Api.StorageTests.Infrastructure;

internal sealed class RequiresR2FactAttribute : FactAttribute
{
    public RequiresR2FactAttribute()
    {
        var missingVariables = R2StorageTestEnvironment.MissingVariables;

        if (missingVariables.Count > 0)
        {
            Skip = $"Requires R2 test configuration: {string.Join(", ", missingVariables)}";
        }
    }
}
