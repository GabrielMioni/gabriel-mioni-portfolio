using Microsoft.EntityFrameworkCore;
using backend.GraphQL;
using backend.Data;
using backend.Features.Projects.Repositories;
using Amazon.S3;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using backend.Features.Aws.Services;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Configure Kestrel to listen on a specific port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// Add CORS services
string corsPolicy = "VueCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, builder =>
    {
        builder
            .WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProjectsRepository, ProjectsRepository>();
builder.Services.AddScoped<IAwsService, AwsService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<RootQuery>()
    .AddMutationType<RootMutation>();

var accessKey = builder.Configuration.GetValue<string>("AWS:AccessKey");
var secretKey = builder.Configuration.GetValue<string>("AWS:SecretKey");
var regionString = builder.Configuration.GetValue<string>("AWS:Region");

var region = RegionEndpoint.GetBySystemName(regionString);

builder.Services.AddScoped<IAmazonS3>(serviceProvider =>
{
    return new AmazonS3Client(accessKey, secretKey, region);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseCors(corsPolicy);
app.MapControllers();
app.MapGraphQL();

app.Run();
