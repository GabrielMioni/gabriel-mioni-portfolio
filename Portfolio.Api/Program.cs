using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Amazon.Runtime;
using Amazon.S3;
using Portfolio.Api.Authentication;
using Portfolio.Api.Configuration;
using Portfolio.Api.Data;
using Portfolio.Api.Infrastructure.Storage;
using Portfolio.Api.Services.Images;
using Portfolio.Api.Services.Projects;
using Portfolio.Api.Services.Storage;
using Portfolio.Api.Services.Tags;
using Portfolio.Api.GraphQL;
using Portfolio.Api.GraphQL.Projects.Admin;
using Portfolio.Api.GraphQL.Tags.Admin;
using Portfolio.Api.GraphQL.Projects.Admin.Types;
using Portfolio.Api.GraphQL.Projects.Public;

var builder = WebApplication.CreateBuilder(args);
var isSchemaCommand = args.Length > 0
    && string.Equals(args[0], "schema", StringComparison.OrdinalIgnoreCase);
var isProductionRuntime = builder.Environment.IsProduction() && !isSchemaCommand;

var clientApplications = builder.Configuration
    .GetSection(ClientApplicationOptions.SectionName)
    .Get<ClientApplicationOptions>() ?? new ClientApplicationOptions();

if (!isSchemaCommand && !clientApplications.IsConfigured)
{
    throw new InvalidOperationException(
        $"{ClientApplicationOptions.SectionName} must define valid AdminOrigin and PublicOrigin URLs.");
}

var adminOrigin = isSchemaCommand
    ? "http://localhost:3000"
    : clientApplications.AdminOrigin.TrimEnd('/');
var publicOrigin = isSchemaCommand
    ? "http://localhost:3001"
    : clientApplications.PublicOrigin.TrimEnd('/');

builder.Services.Configure<ClientApplicationOptions>(
    builder.Configuration.GetSection(ClientApplicationOptions.SectionName));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("client", p => p
      .WithOrigins(adminOrigin)
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials());

    options.AddPolicy("public", p => p
      .WithOrigins(publicOrigin)
      .AllowAnyHeader()
      .AllowAnyMethod());
});

builder.Services.AddDbContextPool<AppDbContext>((services, options) =>
{
    var connectionString = services
        .GetRequiredService<IConfiguration>()
        .GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString);
});

builder.Services.AddPooledDbContextFactory<AppDbContext>((services, options) =>
{
    var connectionString = services
        .GetRequiredService<IConfiguration>()
        .GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString);
});

builder.Services
  .AddIdentityApiEndpoints<IdentityUser>()
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddGitHubAuthentication(
    builder.Configuration,
    requireConfiguration: isProductionRuntime);
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = builder.Environment.IsProduction()
        ? "__Host-PortfolioAdmin"
        : "Portfolio.Admin.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services
    .AddGraphQLServer()
    .AddErrorFilter<UnexpectedGraphQlErrorFilter>()
    .AddQueryType(d => d.Name("Query"))
    .AddTypeExtension<PublicProjectQuery>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

builder.Services
    .AddGraphQLServer("admin")
    .AddErrorFilter<UnexpectedGraphQlErrorFilter>()
    .AddQueryType(d => d.Name("Query"))
    .AddTypeExtension<AdminProjectQuery>()
    .AddTypeExtension<AdminTagQuery>()
    .AddMutationType(d => d.Name("Mutation"))
    .AddTypeExtension<AdminProjectMutation>()
    .AddTypeExtension<AdminProjectImageMutation>()
    .AddTypeExtension<AdminTagMutation>()
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
builder.Services.AddScoped<ProjectImageService>();
builder.Services.AddScoped<ProjectTagService>();

builder.Services.AddOptions<R2Options>()
  .Bind(builder.Configuration.GetSection("R2"))
  .Validate(o => !string.IsNullOrEmpty(o.AccessKey), "R2 AccessKey missing")
  .Validate(o => !string.IsNullOrEmpty(o.SecretKey), "R2 SecretKey missing")
  .ValidateOnStart();

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<R2Options>>().Value;

    var config = new AmazonS3Config
    {
        ServiceURL = opts.Endpoint,
        ForcePathStyle = true,
    };

    var creds = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
    return new AmazonS3Client(creds, config);
});

builder.Services.AddSingleton<IObjectStorage, ObjectStorage>();

var app = builder.Build();
var isEndToEnd = app.Environment.IsEnvironment("EndToEnd");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment() && !isEndToEnd)
{
    app.UseHttpsRedirection();
}
app.UseCors("client");

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/graphql").RequireCors("public");
var adminGraphQl = app
    .MapGraphQL("/graphql/admin", schemaName: "admin")
    .RequireCors("client");

if (!app.Environment.IsDevelopment())
{
    adminGraphQl.RequireAuthorization("Admin");
}

app.MapGitHubAuthentication();

if (isEndToEnd)
{
    app.MapEndToEndAuthentication(app.Configuration);
}

app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        isAuthenticated = user.Identity?.IsAuthenticated ?? false,
        name = user.Identity?.Name
    });
}).RequireAuthorization("Admin");

app.MapControllers();

if (!isSchemaCommand)
{
    if (isEndToEnd)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    await ProjectTagSeed.SeedAsync(app);
}

await app.RunWithGraphQLCommandsAsync(args);

public partial class Program;
