using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiVersioningDemo.IntegrationTests;

public sealed class ApiVersionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiVersionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnsupportedVersion_ReturnsError()
    {
        var response = await _client.GetAsync("/api/v99/products");

        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Expected 400 or 404 for unsupported version, got {(int)response.StatusCode}.");
    }

    [Fact]
    public async Task ProductResponse_IncludesSupportedAndDeprecatedVersionHeaders()
    {
        var v2 = await _client.GetAsync("/api/v2/products/1");
        Assert.Equal(HttpStatusCode.OK, v2.StatusCode);
        Assert.True(
            v2.Headers.TryGetValues("api-supported-versions", out var v2Supported),
            "api-supported-versions header missing on V2.");
        Assert.Contains("2.0", string.Join(",", v2Supported));

        var v1 = await _client.GetAsync("/api/v1/products/1");
        Assert.Equal(HttpStatusCode.OK, v1.StatusCode);
        Assert.True(
            v1.Headers.TryGetValues("api-deprecated-versions", out var v1Deprecated),
            "api-deprecated-versions header missing on V1.");
        Assert.Contains("1.0", string.Join(",", v1Deprecated));

        var system = await _client.GetAsync("/api/v2/system/info");
        Assert.Equal(HttpStatusCode.OK, system.StatusCode);
        Assert.True(system.Headers.TryGetValues("api-supported-versions", out var supported));
        Assert.Contains("2.0", string.Join(",", supported));
        Assert.True(system.Headers.TryGetValues("api-deprecated-versions", out var deprecated));
        Assert.Contains("1.0", string.Join(",", deprecated));
    }

    [Fact]
    public async Task DeprecatedV1_StillWorks_AndReportsDeprecation()
    {
        var response = await _client.GetAsync("/api/v1/products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("api-deprecated-versions", out var deprecated));
        Assert.Contains("1.0", string.Join(",", deprecated));
    }

    [Fact]
    public async Task SystemInfo_MapToApiVersion_ReturnsDifferentContracts()
    {
        var v1 = await _client.GetAsync("/api/v1/system/info");
        var v2 = await _client.GetAsync("/api/v2/system/info");

        Assert.Equal(HttpStatusCode.OK, v1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, v2.StatusCode);

        var v1Body = await v1.Content.ReadAsStringAsync();
        var v2Body = await v2.Content.ReadAsStringAsync();

        Assert.Contains("\"name\"", v1Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"application\"", v2Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"application\"", v1Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConventionController_IsVersioned()
    {
        var response = await _client.GetAsync("/api/v1/convention-demo");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
