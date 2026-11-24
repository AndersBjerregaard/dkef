using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteTextsController(
    SiteTextRepository repository,
    SiteTextRegistry registry,
    ILogger<SiteTextsController> logger
) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{group}")]
    public async Task<IActionResult> GetGroup([FromRoute] string group, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        if (registry.FindGroup(group) is null) return NotFound();

        try
        {
            return Ok(ToResponse(group, await repository.GetGroupAsync(group, cancellationToken)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return PersistenceFailure(ex);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{group}")]
    public async Task<IActionResult> PatchGroup([FromRoute] string group, CancellationToken cancellationToken)
    {
        // Authorization runs before body reading; even an Admin must have an unambiguous Identity ID.
        var updatedBy = SiteTextAuditIdentity.FindIdentityId(User);
        if (updatedBy is null) return Forbid();
        if (registry.FindGroup(group) is null) return NotFound();

        var maximum = SiteTextPatchContract.MaxRequestBytes(registry, group);
        if (Request.ContentLength > maximum) return RequestError(StatusCodes.Status413PayloadTooLarge);

        try
        {
            // Read at most the approved bound plus one sentinel byte, also for chunked bodies.
            // Do not model-bind a dictionary: it would hide duplicate JSON properties from the parser.
            using var body = new MemoryStream();
            var buffer = new byte[8192];
            int read;
            while ((read = await Request.Body.ReadAsync(
                buffer.AsMemory(0, (int)Math.Min(buffer.Length, maximum - body.Length + 1)), cancellationToken)) > 0)
            {
                if (body.Length + read > maximum) return RequestError(StatusCodes.Status413PayloadTooLarge);
                body.Write(buffer, 0, read);
            }

            var validation = SiteTextPatchContract.Validate(
                body.GetBuffer().AsMemory(0, (int)body.Length), registry, group);
            if (validation.Request is null) return BadRequest(validation.ToProblemDetails());

            // The parser already sanitizes and validates every value exactly once. Save only once,
            // and expose the repository's full transactional snapshot only after its commit succeeds.
            var saved = await repository.SaveBatchAsync(group, validation.Request.Changes, updatedBy, cancellationToken);
            return Ok(ToResponse(group, saved));
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status413PayloadTooLarge)
        {
            return RequestError(ex.StatusCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return PersistenceFailure(ex);
        }
    }

    private SiteTextGroupResponse ToResponse(string group, IReadOnlyList<SiteText> rows)
    {
        var fields = registry.GetFields(group);
        return new(group, rows.Where(row => fields.ContainsKey(row.Key))
            .ToDictionary(row => row.Key, row => row.Value, StringComparer.Ordinal));
    }

    private ObjectResult RequestError(int status)
    {
        var problem = new SiteTextPatchValidationResult(null, new Dictionary<string, string[]>
        {
            [SiteTextPatchContract.RequestErrorKey] = [status == StatusCodes.Status413PayloadTooLarge
                ? "Anmodningen er for stor."
                : "Anmodningen indeholder ugyldig JSON."]
        }).ToProblemDetails();
        problem.Status = status;
        return StatusCode(status, problem);
    }

    private ObjectResult PersistenceFailure(Exception exception)
    {
        // Provider exception messages/details can contain SQL, values or audit IDs. Log only the
        // failure type and request trace, never the exception object or submitted body/identity.
        logger.LogError("Site-text operation failed ({FailureType}); trace {TraceId}.",
            exception.GetType().Name, HttpContext.TraceIdentifier);
        return Problem(statusCode: StatusCodes.Status500InternalServerError,
            title: "Teksterne kunne ikke hentes eller gemmes. Prøv igen senere.");
    }
}
