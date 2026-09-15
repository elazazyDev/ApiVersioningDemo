using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.Api.Controllers;

[ApiController]
[ApiVersion(1.0, Deprecated = true)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/system")]
public sealed class SystemInfoController : ControllerBase
{
    [HttpGet("info")]
    [MapToApiVersion(1.0)]
    [ProducesResponseType(typeof(SystemInfoV1Response), StatusCodes.Status200OK)]
    public ActionResult<SystemInfoV1Response> GetInfoV1() =>
        Ok(new SystemInfoV1Response("ApiVersioningDemo", "1.0"));

    [HttpGet("info")]
    [MapToApiVersion(2.0)]
    [ProducesResponseType(typeof(SystemInfoV2Response), StatusCodes.Status200OK)]
    public ActionResult<SystemInfoV2Response> GetInfoV2() =>
        Ok(new SystemInfoV2Response(
            Application: "ApiVersioningDemo",
            ApiVersion: "2.0",
            Environment: HttpContext.RequestServices
                .GetRequiredService<IHostEnvironment>()
                .EnvironmentName));
}

public sealed record SystemInfoV1Response(string Name, string Version);

public sealed record SystemInfoV2Response(string Application, string ApiVersion, string Environment);
