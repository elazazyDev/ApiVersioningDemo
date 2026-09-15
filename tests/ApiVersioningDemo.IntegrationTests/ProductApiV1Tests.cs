using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiVersioningDemo.IntegrationTests;

public sealed class ProductApiV1Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductApiV1Tests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProduct_V1_ReturnsFlatContract_WithoutV2Fields()
    {
        var response = await _client.GetAsync("/api/v1/products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("name", out _));
        Assert.True(root.TryGetProperty("price", out _));
        Assert.False(root.TryGetProperty("displayName", out _));
        Assert.False(root.TryGetProperty("pricing", out _));
        Assert.Equal("Laptop", root.GetProperty("name").GetString());
        Assert.Equal(25_000m, root.GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task GetProducts_V1_ReturnsList()
    {
        var response = await _client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, products.ValueKind);
        Assert.Equal(3, products.GetArrayLength());
    }

    [Fact]
    public async Task GetUnknownProduct_V1_ReturnsProblemDetails()
    {
        var response = await _client.GetAsync("/api/v1/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Product not found", problem.Title);
    }
}
