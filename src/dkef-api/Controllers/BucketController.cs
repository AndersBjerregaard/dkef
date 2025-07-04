using Dkef.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class BucketController(IBucketService _bucketService) : ControllerBase
{
    private const string EVENTS_BUCKET = "events";

    [HttpGet("events/{id}")]
    public async Task<IActionResult> GetPresignedUrl([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out _))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        return Ok(await _bucketService.GetPresignedUrlAsync(EVENTS_BUCKET, id, isPublic: true));
    }
}