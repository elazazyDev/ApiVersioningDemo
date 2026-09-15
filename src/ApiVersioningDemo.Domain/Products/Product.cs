namespace ApiVersioningDemo.Domain.Products;

public sealed class Product
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string Currency { get; init; } = string.Empty;

    public string? Description { get; init; }
}
