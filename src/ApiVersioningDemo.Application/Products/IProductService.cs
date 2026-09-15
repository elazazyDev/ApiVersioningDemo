using ApiVersioningDemo.Domain.Products;

namespace ApiVersioningDemo.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
