using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/convention-demo")]
public sealed class ConventionVersioningController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ConventionDemoResponse), StatusCodes.Status200OK)]
    public ActionResult<ConventionDemoResponse> Get() =>
        Ok(new ConventionDemoResponse(
            Message: "Version configured via MVC API versioning conventions.",
            Technique: "options.Conventions.Controller<T>().HasApiVersion(...)"));
}

public sealed record ConventionDemoResponse(string Message, string Technique);
