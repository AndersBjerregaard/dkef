using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class AttachmentsController(
    IAttachmentsRepository _repository,
    IBucketService _bucketService,
    IAttachmentValidationService _validationService) : ControllerBase
{
    private const string EVENTS_ATTACHMENTS_BUCKET = "events-attachments";
    private const string NEWS_ATTACHMENTS_BUCKET = "news-attachments";
    private const string GENERAL_ASSEMBLIES_ATTACHMENTS_BUCKET = "general-assemblies-attachments";

    // Events Attachments
    [HttpPost("events/{entityId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddEventAttachment([FromRoute] string entityId, [FromBody] AttachmentCreateDto dto)
    {
        return await AddAttachment(entityId, dto, EVENTS_ATTACHMENTS_BUCKET);
    }

    [HttpDelete("events/{entityId}/{attachmentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEventAttachment([FromRoute] string entityId, [FromRoute] string attachmentId)
    {
        return await DeleteAttachment(entityId, attachmentId, EVENTS_ATTACHMENTS_BUCKET);
    }

    [HttpGet("events/{entityId}")]
    public async Task<IActionResult> GetEventAttachments([FromRoute] string entityId)
    {
        return await GetAttachments(entityId);
    }

    // News Attachments
    [HttpPost("news/{entityId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddNewsAttachment([FromRoute] string entityId, [FromBody] AttachmentCreateDto dto)
    {
        return await AddAttachment(entityId, dto, NEWS_ATTACHMENTS_BUCKET);
    }

    [HttpDelete("news/{entityId}/{attachmentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNewsAttachment([FromRoute] string entityId, [FromRoute] string attachmentId)
    {
        return await DeleteAttachment(entityId, attachmentId, NEWS_ATTACHMENTS_BUCKET);
    }

    [HttpGet("news/{entityId}")]
    public async Task<IActionResult> GetNewsAttachments([FromRoute] string entityId)
    {
        return await GetAttachments(entityId);
    }

    // General Assemblies Attachments
    [HttpPost("general-assemblies/{entityId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddGeneralAssemblyAttachment([FromRoute] string entityId, [FromBody] AttachmentCreateDto dto)
    {
        return await AddAttachment(entityId, dto, GENERAL_ASSEMBLIES_ATTACHMENTS_BUCKET);
    }

    [HttpDelete("general-assemblies/{entityId}/{attachmentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGeneralAssemblyAttachment([FromRoute] string entityId, [FromRoute] string attachmentId)
    {
        return await DeleteAttachment(entityId, attachmentId, GENERAL_ASSEMBLIES_ATTACHMENTS_BUCKET);
    }

    [HttpGet("general-assemblies/{entityId}")]
    public async Task<IActionResult> GetGeneralAssemblyAttachments([FromRoute] string entityId)
    {
        return await GetAttachments(entityId);
    }

    // Presigned URL endpoints
    [HttpGet("presigned/events/{fileId}")]
    public async Task<IActionResult> GetEventAttachmentPresignedUrl([FromRoute] string fileId)
    {
        return await GetPresignedUrl(fileId, EVENTS_ATTACHMENTS_BUCKET);
    }

    [HttpGet("presigned/news/{fileId}")]
    public async Task<IActionResult> GetNewsAttachmentPresignedUrl([FromRoute] string fileId)
    {
        return await GetPresignedUrl(fileId, NEWS_ATTACHMENTS_BUCKET);
    }

    [HttpGet("presigned/general-assemblies/{fileId}")]
    public async Task<IActionResult> GetGeneralAssemblyAttachmentPresignedUrl([FromRoute] string fileId)
    {
        return await GetPresignedUrl(fileId, GENERAL_ASSEMBLIES_ATTACHMENTS_BUCKET);
    }

    // Private helper methods
    private async Task<IActionResult> AddAttachment(string entityId, AttachmentCreateDto dto, string bucketName)
    {
        if (!Guid.TryParse(entityId, out var parsedEntityId))
        {
            return BadRequest($"Kunne ikke tolke {entityId} som en GUID.");
        }

        if (string.IsNullOrWhiteSpace(dto.FileName) || string.IsNullOrWhiteSpace(dto.FileId) || string.IsNullOrWhiteSpace(dto.MimeType))
        {
            return BadRequest("Filnavn, fileId og MIME-type er påkrævet.");
        }

        var currentAttachments = await _repository.GetByEntityIdAsync(parsedEntityId);
        var (isCountValid, countError) = _validationService.ValidateAttachmentCount(currentAttachments.Count);
        if (!isCountValid)
        {
            return BadRequest(countError);
        }

        var (isValid, errorMessage) = _validationService.ValidateAttachment(dto.FileName, dto.FileSizeBytes, dto.MimeType);
        if (!isValid)
        {
            return BadRequest(errorMessage);
        }

        try
        {
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                EntityId = parsedEntityId,
                FileName = Path.GetFileNameWithoutExtension(dto.FileName),
                FileId = dto.FileId,
                FileSizeBytes = dto.FileSizeBytes,
                MimeType = dto.MimeType,
                CreatedAt = DateTime.UtcNow,
                OriginalFileName = dto.FileName,
            };

            await _repository.CreateAsync(attachment);
            return CreatedAtAction(nameof(GetEventAttachments), new { entityId }, attachment);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Fejl ved oprettelse af vedhæftelse: {ex.Message}");
        }
    }

    private async Task<IActionResult> DeleteAttachment(string entityId, string attachmentId, string bucketName)
    {
        if (!Guid.TryParse(entityId, out var parsedEntityId) || !Guid.TryParse(attachmentId, out var parsedAttachmentId))
        {
            return BadRequest("Ugyldige GUIDs.");
        }

        try
        {
            var attachment = await _repository.GetByIdAsync(parsedAttachmentId);
            if (attachment == null)
            {
                return NotFound("Vedhæftelse blev ikke fundet.");
            }

            if (attachment.EntityId != parsedEntityId)
            {
                return Forbid();
            }

            // Delete from MinIO fire-and-forget
            _ = _bucketService.DeleteObjectAsync(bucketName, attachment.FileId);
            
            await _repository.DeleteAsync(parsedAttachmentId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Fejl ved sletning af fil: {ex.Message}");
        }
    }

    private async Task<IActionResult> GetAttachments(string entityId)
    {
        if (!Guid.TryParse(entityId, out var parsedEntityId))
        {
            return BadRequest($"Kunne ikke tolke {entityId} som en GUID.");
        }

        var attachments = await _repository.GetByEntityIdAsync(parsedEntityId);
        return Ok(attachments);
    }

    private async Task<IActionResult> GetPresignedUrl(string fileId, string bucketName)
    {
        if (!Guid.TryParse(fileId, out _))
        {
            return BadRequest($"Kunne ikke tolke {fileId} som en GUID.");
        }

        try
        {
            var presignedUrl = await _bucketService.GetPresignedUrlAsync(bucketName, fileId, isPublic: true);
            return Ok(presignedUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Fejl ved hentning af download-URL: {ex.Message}");
        }
    }
}
