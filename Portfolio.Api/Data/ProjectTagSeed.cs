using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Data;

public static class ProjectTagSeed
{
    public static async Task SeedAsync(WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        if (await db.Tags.AnyAsync())
            return;

        var tags = new[]
        {
            ProjectTag.Create("Vue"),
            ProjectTag.Create("TypeScript"),
            ProjectTag.Create("Nuxt"),
            ProjectTag.Create(".NET"),
            ProjectTag.Create("GraphQL"),
            ProjectTag.Create("C#"),
        };

        db.Tags.AddRange(tags);
        await db.SaveChangesAsync();
    }
}
