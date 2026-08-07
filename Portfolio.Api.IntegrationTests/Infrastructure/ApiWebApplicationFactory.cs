using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.IntegrationTests.Infrastructure;

public sealed class ApiWebApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    internal FakeObjectStorage ObjectStorage { get; } = new();

    public HttpClient CreateAuthenticatedClient(bool isAdmin = true)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserHeader,
            isAdmin ? "integration-test-admin" : "integration-test-user");

        if (isAdmin)
        {
            client.DefaultRequestHeaders.Add(
                TestAuthenticationHandler.RoleHeader,
                "Admin");
        }

        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["ClientApplications:AdminOrigin"] = "http://localhost:3000",
                ["ClientApplications:PublicOrigin"] = "http://localhost:3001",
                ["R2:AccessKey"] = "integration-test-access-key",
                ["R2:SecretKey"] = "integration-test-secret-key"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IObjectStorage>();
            services.AddSingleton<IObjectStorage>(ObjectStorage);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
        });
    }
}
