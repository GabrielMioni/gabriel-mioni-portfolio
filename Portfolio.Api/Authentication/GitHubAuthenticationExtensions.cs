using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Portfolio.Api.Data;

namespace Portfolio.Api.Authentication;

public static class GitHubAuthenticationExtensions
{
    private const string GitHubScheme = "GitHub";
    private const string AdminRole = "Admin";

    public static IServiceCollection AddGitHubAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(GitHubAuthenticationOptions.SectionName);

        services.Configure<GitHubAuthenticationOptions>(section);

        services
            .AddAuthentication()
            .AddOAuth(GitHubScheme, options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = section[nameof(GitHubAuthenticationOptions.ClientId)] ?? "not-configured";
                options.ClientSecret = section[nameof(GitHubAuthenticationOptions.ClientSecret)] ?? "not-configured";
                options.CallbackPath = "/api/auth/github/callback";
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.SaveTokens = false;

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = PopulateGitHubClaimsAsync,
                    OnRemoteFailure = HandleRemoteFailureAsync
                };
            });

        return services;
    }

    public static IEndpointRouteBuilder MapGitHubAuthentication(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/auth/github/login", (
            string? returnUrl,
            SignInManager<IdentityUser> signInManager,
            IOptions<GitHubAuthenticationOptions> options) =>
        {
            if (!options.Value.IsConfigured)
            {
                return Results.Problem(
                    title: "GitHub authentication is not configured.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var returnPath = NormalizeReturnPath(returnUrl);
            var callbackPath = $"/api/auth/github/complete?returnUrl={Uri.EscapeDataString(returnPath)}";
            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                GitHubScheme,
                callbackPath);

            return Results.Challenge(properties, [GitHubScheme]);
        });

        endpoints.MapGet("/api/auth/github/complete", async (
            string? returnUrl,
            HttpContext httpContext,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db,
            IOptions<GitHubAuthenticationOptions> options,
            ILoggerFactory loggerFactory) =>
        {
            var authOptions = options.Value;
            var logger = loggerFactory.CreateLogger("GitHubAuthentication");

            if (!authOptions.IsConfigured)
            {
                return Results.Problem(
                    title: "GitHub authentication is not configured.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var loginPath = BuildAdminUrl(authOptions.AdminBaseUrl, "/login?error=github_authentication_failed");
            var externalLogin = await signInManager.GetExternalLoginInfoAsync();

            if (externalLogin is null)
            {
                logger.LogWarning("GitHub authentication completed without external login information.");
                return await RedirectAfterExternalSignOutAsync(httpContext, loginPath);
            }

            if (!string.Equals(
                    externalLogin.ProviderKey,
                    authOptions.AllowedUserId,
                    StringComparison.Ordinal))
            {
                logger.LogWarning(
                    "GitHub user {GitHubUserId} attempted to access the admin application.",
                    externalLogin.ProviderKey);
                return await RedirectAfterExternalSignOutAsync(httpContext, BuildAdminUrl(
                    authOptions.AdminBaseUrl,
                    "/login?error=github_account_not_allowed"));
            }

            await using var transaction = await db.Database.BeginTransactionAsync(
                httpContext.RequestAborted);

            var user = await userManager.FindByLoginAsync(
                externalLogin.LoginProvider,
                externalLogin.ProviderKey);

            if (user is null)
            {
                var gitHubLogin = externalLogin.Principal.FindFirstValue(ClaimTypes.Name);
                user = new IdentityUser
                {
                    UserName = string.IsNullOrWhiteSpace(gitHubLogin)
                        ? $"github-{externalLogin.ProviderKey}"
                        : gitHubLogin
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    LogIdentityErrors(logger, "create the GitHub admin user", createResult);
                    return await RedirectAfterExternalSignOutAsync(httpContext, loginPath);
                }

                var addLoginResult = await userManager.AddLoginAsync(user, externalLogin);
                if (!addLoginResult.Succeeded)
                {
                    LogIdentityErrors(logger, "link the GitHub admin login", addLoginResult);
                    return await RedirectAfterExternalSignOutAsync(httpContext, loginPath);
                }
            }

            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole(AdminRole));
                if (!createRoleResult.Succeeded)
                {
                    LogIdentityErrors(logger, "create the Admin role", createRoleResult);
                    return await RedirectAfterExternalSignOutAsync(httpContext, loginPath);
                }
            }

            if (!await userManager.IsInRoleAsync(user, AdminRole))
            {
                var addRoleResult = await userManager.AddToRoleAsync(user, AdminRole);
                if (!addRoleResult.Succeeded)
                {
                    LogIdentityErrors(logger, "assign the Admin role", addRoleResult);
                    return await RedirectAfterExternalSignOutAsync(httpContext, loginPath);
                }
            }

            await transaction.CommitAsync(httpContext.RequestAborted);
            await signInManager.SignInAsync(user, isPersistent: true, GitHubScheme);
            await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return Results.Redirect(BuildAdminUrl(
                authOptions.AdminBaseUrl,
                NormalizeReturnPath(returnUrl)));
        });

        endpoints.MapPost("/api/auth/logout", async (
            SignInManager<IdentityUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        return endpoints;
    }

    private static Task HandleRemoteFailureAsync(RemoteFailureContext context)
    {
        var authOptions = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<GitHubAuthenticationOptions>>()
            .Value;
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GitHubAuthentication");

        logger.LogWarning(context.Failure, "GitHub authentication failed.");

        var redirectPath = authOptions.IsConfigured
            ? BuildAdminUrl(
                authOptions.AdminBaseUrl,
                "/login?error=github_authentication_failed")
            : "/";

        context.Response.Redirect(redirectPath);
        context.HandleResponse();

        return Task.CompletedTask;
    }

    private static async Task PopulateGitHubClaimsAsync(OAuthCreatingTicketContext context)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            context.Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            context.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.ParseAdd("gabriel-mioni-portfolio");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        using var response = await context.Backchannel.SendAsync(
            request,
            context.HttpContext.RequestAborted);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(
            context.HttpContext.RequestAborted);
        using var user = await JsonDocument.ParseAsync(
            responseStream,
            cancellationToken: context.HttpContext.RequestAborted);

        context.RunClaimActions(user.RootElement);
    }

    private static string NormalizeReturnPath(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)
            || !returnUrl.StartsWith('/')
            || returnUrl.StartsWith("//", StringComparison.Ordinal)
            || returnUrl.StartsWith("/\\", StringComparison.Ordinal))
        {
            return "/";
        }

        return returnUrl;
    }

    private static string BuildAdminUrl(string adminBaseUrl, string path)
    {
        var baseUri = new Uri(adminBaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(baseUri, path.TrimStart('/')).ToString();
    }

    private static async Task<IResult> RedirectAfterExternalSignOutAsync(
        HttpContext httpContext,
        string redirectPath)
    {
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Results.Redirect(redirectPath);
    }

    private static void LogIdentityErrors(
        ILogger logger,
        string operation,
        IdentityResult result)
    {
        logger.LogError(
            "Failed to {Operation}: {Errors}",
            operation,
            string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
