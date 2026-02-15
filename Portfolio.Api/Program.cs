using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.GraphQL.Projects;
using Portfolio.Api.GraphQL.Projects.Types;
using Portfolio.Api.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("client", p => p
      .WithOrigins("http://localhost:3000")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials());
});

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextPool<AppDbContext>(options =>
  options.UseSqlServer(conn));

builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
  options.UseSqlServer(conn));

builder.Services
  .AddIdentityApiEndpoints<IdentityUser>()
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<ProjectQuery>()
    .AddMutationType<ProjectMutation>()
    .AddType<ProjectType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyCostOptions(o =>
    {
        o.MaxFieldCost = 5000;
        o.Sorting.VariableMultiplier = 1;
        o.Filtering.VariableMultiplier = 1;
    });

builder.Services.AddScoped<ProjectService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("client");

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        isAuthenticated = user.Identity?.IsAuthenticated ?? false,
        name = user.Identity?.Name
    });
}).RequireAuthorization();

app.MapGet("/api/user", (ClaimsPrincipal user) =>
{
    if (!(user.Identity?.IsAuthenticated ?? false))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        isAuthenticated = true,
        name = user.Identity?.Name,
        claims = user.Claims.Select(c => new { c.Type, c.Value })
    });
}).RequireAuthorization();

app.MapControllers();

// Block registration
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.Equals("/api/auth/register", StringComparison.OrdinalIgnoreCase))
    {
        if (!(ctx.User?.Identity?.IsAuthenticated ?? false) || !ctx.User.IsInRole("Admin"))
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
    }

    await next();
});

await IdentitySeed.SeedAdminAsync(app);

app.Run();
