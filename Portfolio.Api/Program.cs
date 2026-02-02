using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;

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

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
  .AddIdentityApiEndpoints<IdentityUser>()
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("client");

app.UseAuthentication();
app.UseAuthorization();

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

app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

// app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" })).RequireAuthorization();
app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }));

app.MapControllers();

// Seeding can be here or right after Build(); either is fine
await IdentitySeed.SeedAdminAsync(app);

app.Run();
