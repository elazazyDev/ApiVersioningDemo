using ApiVersioningDemo.Domain.Products;

namespace ApiVersioningDemo.Application.Products;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        repository.GetByIdAsync(id, cancellationToken);
}
