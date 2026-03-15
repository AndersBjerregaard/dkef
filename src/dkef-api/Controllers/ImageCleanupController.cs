using Dkef.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

/// <summary>
/// Controller for manual image cleanup operations.
/// These endpoints are only available in DEBUG mode for testing and maintenance.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ImageCleanupController(ImageCleanupService _cleanupService) : ControllerBase
{
#if DEBUG
    /// <summary>
    /// Scans MinIO buckets for orphaned images (images not referenced by any database records)
    /// and deletes them. Available only in DEBUG mode.
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<IActionResult> CleanupOrphanImages()
    {
        try
        {
            Console.WriteLine("\n=== Starting image cleanup via API ===");
            var result = await _cleanupService.CleanupOrphanImagesAsync();
            Console.WriteLine(result.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Cleanup failed: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
#endif
}
