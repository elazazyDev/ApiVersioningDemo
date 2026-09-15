using ApiVersioningDemo.Domain.Products;
using V1 = ApiVersioningDemo.Api.Contracts.V1;
using V2 = ApiVersioningDemo.Api.Contracts.V2;

namespace ApiVersioningDemo.Api.Mappings;

public static class ProductMappings
{
    public static V1.ProductResponse ToV1Response(this Product product) =>
        new(product.Id, product.Name, product.Price);

    public static V2.ProductResponse ToV2Response(this Product product) =>
        new(
            product.Id,
            product.Name,
            new V2.PriceResponse(product.Price, product.Currency),
            product.Description);
}
