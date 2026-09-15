using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiVersioningDemo.IntegrationTests;

public sealed class ProductApiV2Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductApiV2Tests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProduct_V2_ReturnsNestedPricingContract()
    {
        var response = await _client.GetAsync("/api/v2/products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("displayName", out var displayName));
        Assert.Equal("Laptop", displayName.GetString());

        Assert.True(root.TryGetProperty("pricing", out var pricing));
        Assert.True(pricing.TryGetProperty("amount", out var amount));
        Assert.True(pricing.TryGetProperty("currency", out var currency));
        Assert.Equal(25_000m, amount.GetDecimal());
        Assert.Equal("EGP", currency.GetString());

        Assert.True(root.TryGetProperty("description", out _));
        Assert.False(root.TryGetProperty("name", out _));
        Assert.False(root.TryGetProperty("price", out _));
    }
}
