using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("client", p => p
        .WithOrigins("http://localhost:3000") // Nuxt dev server)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization();

builder.Services
  .AddIdentityApiEndpoints<IdentityUser>()
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

app.MapIdentityApi<IdentityUser>();

await IdentitySeed.SeedAdminAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("client");

app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }))
    .RequireAuthorization();


app.MapControllers();

app.Run();
