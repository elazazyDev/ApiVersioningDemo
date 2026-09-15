namespace ApiVersioningDemo.Api.Contracts.V2;

public sealed record PriceResponse(
    decimal Amount,
    string Currency);
