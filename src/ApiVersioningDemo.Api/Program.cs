using Asp.Versioning;
using ApiVersioningDemo.Api.Controllers;
using ApiVersioningDemo.Api.MinimalApis;
using ApiVersioningDemo.Application.Products;
using ApiVersioningDemo.Domain.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();

builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();

        options.Policies
            .Deprecate(1.0)
            .Effective(new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero));

        options.Policies
            .Sunset(1.0)
            .Effective(new DateTimeOffset(2027, 6, 1, 0, 0, 0, TimeSpan.Zero))
            .Link("https://localhost:7211/swagger/index.html?urls.primaryName=v2")
            .Title("Migration Guide to API V2")
            .Type("text/html");
    })
    .AddMvc(options =>
    {
        var convention = options.Conventions.Controller<ConventionVersioningController>();
        convention.HasApiVersion(new ApiVersion(1, 0));
        convention.HasApiVersion(new ApiVersion(2, 0));
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    })
    .AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();

    app.UseSwaggerUI(options =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            options.SwaggerEndpoint(
                $"/openapi/{description.GroupName}.json",
                description.GroupName);
        }
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapOrderEndpoints();

app.Run();

public partial class Program;
