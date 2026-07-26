using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Xunit;

namespace Portfolio.Api.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public sealed class DatabaseIsolationTests(SqlServerFixture database)
{
    [Fact]
    public async Task ApiWebApplicationFactory_UsesContainerDatabaseForAllContexts()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);

        var expectedOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(database.ConnectionString)
            .Options;

        await using var expectedDb = new AppDbContext(expectedOptions);
        var expectedConnection = expectedDb.Database.GetDbConnection();

        await using var scope = factory.Services.CreateAsyncScope();

        var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        AssertSameDatabase(
            expectedConnection,
            scopedDb.Database.GetDbConnection());

        var dbFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<AppDbContext>>();

        await using var factoryDb = await dbFactory.CreateDbContextAsync();
        AssertSameDatabase(
            expectedConnection,
            factoryDb.Database.GetDbConnection());
    }

    private static void AssertSameDatabase(
        System.Data.Common.DbConnection expected,
        System.Data.Common.DbConnection actual)
    {
        Assert.Equal(expected.DataSource, actual.DataSource);
        Assert.Equal(expected.Database, actual.Database);
    }
}
