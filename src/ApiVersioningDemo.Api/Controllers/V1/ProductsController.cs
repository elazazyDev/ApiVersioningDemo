using Asp.Versioning;
using ApiVersioningDemo.Api.Contracts.V1;
using ApiVersioningDemo.Api.Mappings;
using ApiVersioningDemo.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0, Deprecated = true)]
[Route("api/v{version:apiVersion}/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(products.Select(p => p.ToV1Response()).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product not found",
                Detail = $"No product exists with id '{id}'.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(product.ToV1Response());
    }
}
