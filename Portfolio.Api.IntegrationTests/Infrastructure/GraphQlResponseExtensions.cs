using System.Text.Json;
using Xunit;

namespace Portfolio.Api.IntegrationTests.Infrastructure;

internal static class GraphQlResponseExtensions
{
    public static async Task<JsonElement> ReadGraphQlDataAsync(
        this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();

            Assert.Fail(
                $"GraphQL request returned {(int)response.StatusCode} " +
                $"({response.StatusCode}).{Environment.NewLine}{errorBody}");
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        return root.GetProperty("data").Clone();
    }

    public static async Task<JsonElement> ReadGraphQlPayloadAsync(
        this HttpResponseMessage response,
        string payloadName)
    {
        var data = await response.ReadGraphQlDataAsync();

        return data.GetProperty(payloadName);
    }
}
