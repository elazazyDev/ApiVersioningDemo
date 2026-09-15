namespace ApiVersioningDemo.Api.Contracts.V1;

public sealed record ProductResponse(
    int Id,
    string Name,
    decimal Price);
