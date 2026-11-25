using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

public sealed record SiteImageResponse(string Url, string Alt);
public sealed record SiteImageGroupResponse(string Group, IReadOnlyDictionary<string, SiteImageResponse> Overrides);

[ApiController]
[Route("[controller]")]
public sealed class SiteImagesController(ISiteImageRepository repository, ISiteImageStorage storage,
    ILogger<SiteImagesController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{group}")]
    public async Task<IActionResult> GetGroup(string group, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        if (!SiteImageRegistry.HasGroup(group)) return NotFound();
        try
        {
            var rows = await repository.GetGroupAsync(group, ct);
            return Ok(new SiteImageGroupResponse(group, rows.Where(row => SiteImageRegistry.HasSlot(group, row.Slot))
                .ToDictionary(row => row.Slot, row => new SiteImageResponse(storage.PublicUrl(row.ObjectKey), row.Alt))));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            return Failure(ex);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{group}/{slot}")]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(SiteImageValidator.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = SiteImageValidator.MaxRequestBytes, ValueLengthLimit = 2048)]
    public async Task<IActionResult> Put(string group, string slot, CancellationToken ct)
    {
        // Read multipart only after authorization, slot and identity checks.
        var identity = SiteTextAuditIdentity.FindIdentityId(User);
        if (identity is null) return Forbid();
        if (!SiteImageRegistry.HasSlot(group, slot)) return NotFound();
        if (Request.ContentLength > SiteImageValidator.MaxRequestBytes) return TooLarge();
        try
        {
            if (!Request.HasFormContentType) return BadRequest(new { title = "Vælg en billedfil og en alternativ billedtekst." });
            var form = await Request.ReadFormAsync(ct);
            if (form.Files.Count != 1 || form.Files[0].Name != "file" ||
                form.Count != 1 || form["alt"].Count != 1)
                return BadRequest(new { title = "Vælg én billedfil og én alternativ billedtekst." });
            var alt = SiteImageValidator.ValidateAlt(form["alt"][0]);
            var file = form.Files[0];
            if (file.Length > SiteImageValidator.MaxFileBytes) return TooLarge();
            if (file.Length == 0) return BadRequest(new { title = "Billedfilen er tom." });
            await using var input = new MemoryStream();
            await file.CopyToAsync(input, ct);
            input.Position = 0;
            await using var encoded = await SiteImageValidator.ValidateAndEncodeAsync(input, ct);
            var key = $"{group}/{slot}/{Guid.NewGuid():N}.webp";
            await storage.UploadAsync(key, encoded, ct);
            // Do not delete the new object on a database exception: commit outcome may be unknown.
            // A leftover file is safer than deleting an image that was actually published.
            var previous = await repository.ReplaceAsync(group, slot, new SiteImage
            {
                Group = group, Slot = slot, ObjectKey = key, Alt = alt,
                UpdatedAt = DateTime.UtcNow, UpdatedBy = identity
            }, ct);
            await CleanupAsync(previous);
            return Ok(new SiteImageResponse(storage.PublicUrl(key), alt));
        }
        catch (SiteImageValidationException ex) { return BadRequest(new { title = ex.Message }); }
        catch (BadHttpRequestException ex) when (ex.StatusCode == 413) { return TooLarge(); }
        catch (InvalidDataException) { return BadRequest(new { title = "Billedoverførslen er ugyldig eller for stor." }); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Failure(ex); }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{group}/{slot}")]
    public async Task<IActionResult> Delete(string group, string slot, CancellationToken ct)
    {
        if (SiteTextAuditIdentity.FindIdentityId(User) is null) return Forbid();
        if (!SiteImageRegistry.HasSlot(group, slot)) return NotFound();
        try
        {
            var previous = await repository.ReplaceAsync(group, slot, null, ct);
            await CleanupAsync(previous);
            return NoContent();
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Failure(ex); }
    }

    private async Task CleanupAsync(string? key)
    {
        if (key is null) return;
        try { await storage.DeleteAsync(key); }
        catch (Exception ex)
        {
            logger.LogWarning("Site-image cleanup failed ({FailureType}); trace {TraceId}.", ex.GetType().Name, HttpContext.TraceIdentifier);
        }
    }

    private ObjectResult TooLarge() => StatusCode(413, new { title = "Billedfilen må højst fylde 5 MB." });

    private ObjectResult Failure(Exception ex)
    {
        logger.LogError("Site-image operation failed ({FailureType}); trace {TraceId}.", ex.GetType().Name, HttpContext.TraceIdentifier);
        return Problem(statusCode: 500, title: "Billedet kunne ikke hentes eller gemmes. Prøv igen senere.");
    }
}
