namespace Portfolio.Api.IntegrationTests.Infrastructure;

internal static class TestData
{
    public static string NewSuffix() =>
        Guid.NewGuid().ToString("N")[..8];
}
