using Asp.Versioning;

namespace ApiVersioningDemo.Api.MinimalApis;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orderVersions = app.NewApiVersionSet("Orders")
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();

        var orders = app.MapGroup("api/v{version:apiVersion}/orders")
            .WithApiVersionSet(orderVersions)
            .WithTags("Orders");

        orders.MapGet("/", () => Results.Ok(new OrderResponseV1(
                Id: 1001,
                Customer: "Alice",
                Total: 499.99m)))
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithName("GetOrdersV1")
            .Produces<OrderResponseV1>();

        orders.MapGet("/", () => Results.Ok(new OrderResponseV2(
                Id: 1001,
                CustomerName: "Alice",
                Cost: new OrderCostResponse(499.99m, "EGP"),
                Status: "Confirmed")))
            .MapToApiVersion(new ApiVersion(2, 0))
            .WithName("GetOrdersV2")
            .Produces<OrderResponseV2>();

        return app;
    }
}

public sealed record OrderResponseV1(int Id, string Customer, decimal Total);

public sealed record OrderCostResponse(decimal Amount, string Currency);

public sealed record OrderResponseV2(
    int Id,
    string CustomerName,
    OrderCostResponse Cost,
    string Status);
