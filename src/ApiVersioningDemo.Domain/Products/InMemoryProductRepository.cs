namespace ApiVersioningDemo.Domain.Products;

public sealed class InMemoryProductRepository : IProductRepository
{
    private static readonly IReadOnlyList<Product> Products =
    [
        new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 25_000m,
            Currency = "EGP",
            Description = "14-inch business laptop"
        },
        new Product
        {
            Id = 2,
            Name = "Mobile Phone",
            Price = 12_500m,
            Currency = "EGP",
            Description = "5G smartphone"
        },
        new Product
        {
            Id = 3,
            Name = "Headphones",
            Price = 1_800m,
            Currency = "EGP",
            Description = "Wireless over-ear headphones"
        }
    ];

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Products);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = Products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }
}
