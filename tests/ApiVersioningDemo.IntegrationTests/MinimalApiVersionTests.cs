using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiVersioningDemo.IntegrationTests;

public sealed class MinimalApiVersionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MinimalApiVersionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Orders_V1_And_V2_ReturnDifferentContracts()
    {
        var v1Response = await _client.GetAsync("/api/v1/orders");
        var v2Response = await _client.GetAsync("/api/v2/orders");

        Assert.Equal(HttpStatusCode.OK, v1Response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, v2Response.StatusCode);

        await using var v1Stream = await v1Response.Content.ReadAsStreamAsync();
        await using var v2Stream = await v2Response.Content.ReadAsStreamAsync();
        using var v1 = await JsonDocument.ParseAsync(v1Stream);
        using var v2 = await JsonDocument.ParseAsync(v2Stream);

        Assert.True(v1.RootElement.TryGetProperty("customer", out _));
        Assert.True(v1.RootElement.TryGetProperty("total", out _));
        Assert.False(v1.RootElement.TryGetProperty("cost", out _));
        Assert.False(v1.RootElement.TryGetProperty("customerName", out _));

        Assert.True(v2.RootElement.TryGetProperty("customerName", out _));
        Assert.True(v2.RootElement.TryGetProperty("cost", out var cost));
        Assert.True(cost.TryGetProperty("amount", out _));
        Assert.True(cost.TryGetProperty("currency", out _));
        Assert.True(v2.RootElement.TryGetProperty("status", out _));
        Assert.False(v2.RootElement.TryGetProperty("total", out _));
    }
}
