using System.Text.Json;
using Xunit;

namespace Portfolio.Api.IntegrationTests.Infrastructure;

internal static class GraphQlAssertions
{
    public static void AssertSingleUserError(
        this JsonElement payload,
        string code,
        string message,
        params string[] field)
    {
        var userError = Assert.Single(
            payload.GetProperty("userErrors").EnumerateArray());

        Assert.Equal(code, userError.GetProperty("code").GetString());
        Assert.Equal(message, userError.GetProperty("message").GetString());

        var actualField = userError.GetProperty("field");

        if (field.Length == 0)
        {
            Assert.Equal(JsonValueKind.Null, actualField.ValueKind);
            return;
        }

        Assert.Equal(
            field,
            actualField
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray());
    }
}
