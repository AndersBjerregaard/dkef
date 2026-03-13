using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class HealthController : ControllerBase {
    [HttpGet]
    public IActionResult Get() => Ok();
}