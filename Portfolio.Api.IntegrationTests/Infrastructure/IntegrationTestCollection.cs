using Xunit;

namespace Portfolio.Api.IntegrationTests.Infrastructure;

public static class IntegrationTestCollection
{
    public const string Name = "API integration tests";
}

[CollectionDefinition(IntegrationTestCollection.Name)]
public sealed class IntegrationTestCollectionDefinition
    : ICollectionFixture<SqlServerFixture>;
