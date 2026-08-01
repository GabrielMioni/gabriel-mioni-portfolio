using Portfolio.Api.Infrastructure.Storage;
using DotNetEnv;

namespace Portfolio.Api.StorageTests.Infrastructure;

internal static class R2StorageTestEnvironment
{
    private const string EnvironmentFileName = ".env.test.local";
    private const string AccessKeyVariable = "R2_TEST_ACCESS_KEY";
    private const string SecretKeyVariable = "R2_TEST_SECRET_KEY";
    private const string EndpointVariable = "R2_TEST_ENDPOINT";
    private const string BucketVariable = "R2_TEST_BUCKET";

    private static readonly string[] RequiredVariables =
    [
        AccessKeyVariable,
        SecretKeyVariable,
        EndpointVariable,
        BucketVariable
    ];

    static R2StorageTestEnvironment()
    {
        var environmentFile = FindEnvironmentFile();

        if (environmentFile is not null)
        {
            Env.NoClobber().Load(environmentFile);
        }
    }

    public static IReadOnlyList<string> MissingVariables => RequiredVariables
        .Where(variable => string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable(variable)))
        .ToList();

    public static R2Options LoadOptions()
    {
        var missingVariables = MissingVariables;

        if (missingVariables.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing R2 test configuration: {string.Join(", ", missingVariables)}");
        }

        return new R2Options
        {
            AccessKey = GetRequiredVariable(AccessKeyVariable),
            SecretKey = GetRequiredVariable(SecretKeyVariable),
            Endpoint = GetRequiredVariable(EndpointVariable),
            Bucket = GetRequiredVariable(BucketVariable),
            PublicBaseUrl = "https://storage.test.invalid"
        };
    }

    private static string GetRequiredVariable(string name)
        => Environment.GetEnvironmentVariable(name)!;

    private static string? FindEnvironmentFile()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                EnvironmentFileName);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
