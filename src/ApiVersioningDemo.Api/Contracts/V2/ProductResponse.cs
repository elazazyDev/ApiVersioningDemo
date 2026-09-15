namespace ApiVersioningDemo.Api.Contracts.V2;

public sealed record ProductResponse(
    int Id,
    string DisplayName,
    PriceResponse Pricing,
    string? Description);
