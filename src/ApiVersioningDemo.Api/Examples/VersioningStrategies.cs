using Asp.Versioning;

namespace ApiVersioningDemo.Api.Examples;

public static class VersioningStrategies
{
    public static IApiVersionReader QueryString() =>
        new QueryStringApiVersionReader("api-version");

    public static IApiVersionReader Header() =>
        new HeaderApiVersionReader("X-Api-Version");

    public static IApiVersionReader MediaType() =>
        new MediaTypeApiVersionReader();

    public static IApiVersionReader CombinedForMigration() =>
        ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Api-Version"));

    public static void ConfigureQueryStringVersioning(ApiVersioningOptions options)
    {
        options.ReportApiVersions = true;
        options.ApiVersionReader = QueryString();
    }

    public static void ConfigureHeaderVersioning(ApiVersioningOptions options)
    {
        options.ReportApiVersions = true;
        options.ApiVersionReader = Header();
    }

    public static void ConfigureMediaTypeVersioning(ApiVersioningOptions options)
    {
        options.ReportApiVersions = true;
        options.ApiVersionReader = MediaType();
    }

    public static void ConfigureCombinedReaders(ApiVersioningOptions options)
    {
        options.ReportApiVersions = true;
        options.ApiVersionReader = CombinedForMigration();
    }
}
