using Dkef.Services;

using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class BucketController(IBucketService _bucketService) : ControllerBase
{
    private const string EVENTS_BUCKET = "events";
    private const string NEWS_BUCKET = "news";
    private const string GENERAL_ASSEMBLIES_BUCKET = "general-assemblies";
    private const string ATTACHMENTS_BUCKET = "attachments";

    [HttpGet("events/{id}")]
    public async Task<IActionResult> GetPresignedUrl([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out _))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        return Ok(await _bucketService.GetPresignedUrlAsync(EVENTS_BUCKET, id, isPublic: true));
    }

    [HttpGet("news/{id}")]
    public async Task<IActionResult> GetNewsPresignedUrl([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out _))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        return Ok(await _bucketService.GetPresignedUrlAsync(NEWS_BUCKET, id, isPublic: true));
    }

    [HttpGet("general-assemblies/{id}")]
    public async Task<IActionResult> GetGeneralAssemblyPresignedUrl([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out _))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        return Ok(await _bucketService.GetPresignedUrlAsync(GENERAL_ASSEMBLIES_BUCKET, id, isPublic: true));
    }

    [HttpGet("attachments/{id}")]
    public async Task<IActionResult> GetAttachmentPresignedUrl([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out _))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        return Ok(await _bucketService.GetPresignedUrlAsync(ATTACHMENTS_BUCKET, id, isPublic: true));
    }
}
