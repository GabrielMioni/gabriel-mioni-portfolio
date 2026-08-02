using Microsoft.AspNetCore.Identity;

namespace Portfolio.Api.Authentication;

public static class EndToEndAuthenticationExtensions
{
    private const string AuthenticationTokenHeader = "X-E2E-Auth-Token";
    private const string AdminAccess = "admin";
    private const string UserAccess = "user";
    private const string AdminRole = "Admin";

    public static IEndpointRouteBuilder MapEndToEndAuthentication(
        this IEndpointRouteBuilder endpoints,
        IConfiguration configuration)
    {
        var expectedToken = configuration["E2E_AUTH_TOKEN"];

        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            throw new InvalidOperationException(
                "E2E_AUTH_TOKEN must be configured in the EndToEnd environment.");
        }

        endpoints.MapPost("/api/auth/e2e/login/{access}", async (
            string access,
            HttpRequest request,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager) =>
        {
            if (!request.Headers.TryGetValue(AuthenticationTokenHeader, out var providedToken)
                || !string.Equals(
                    providedToken.ToString(),
                    expectedToken,
                    StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            var isAdmin = access switch
            {
                AdminAccess => true,
                UserAccess => false,
                _ => (bool?)null
            };

            if (isAdmin is null)
            {
                return Results.BadRequest();
            }

            var username = isAdmin.Value ? "e2e-admin" : "e2e-user";
            var user = await userManager.FindByNameAsync(username);

            if (user is null)
            {
                user = new IdentityUser { UserName = username };
                EnsureSucceeded(
                    await userManager.CreateAsync(user),
                    $"create {username}");
            }

            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                EnsureSucceeded(
                    await roleManager.CreateAsync(new IdentityRole(AdminRole)),
                    "create the Admin role");
            }

            var isInAdminRole = await userManager.IsInRoleAsync(user, AdminRole);

            if (isAdmin.Value && !isInAdminRole)
            {
                EnsureSucceeded(
                    await userManager.AddToRoleAsync(user, AdminRole),
                    $"assign {username} to the Admin role");
            }
            else if (!isAdmin.Value && isInAdminRole)
            {
                EnsureSucceeded(
                    await userManager.RemoveFromRoleAsync(user, AdminRole),
                    $"remove {username} from the Admin role");
            }

            await signInManager.SignInAsync(
                user,
                isPersistent: false,
                authenticationMethod: "EndToEnd");

            return Results.Ok(new
            {
                user.UserName,
                IsAdmin = isAdmin.Value
            });
        });

        return endpoints;
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Failed to {operation}: "
            + string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
