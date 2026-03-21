using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Extensions;
using Dkef.Repositories;
using Dkef.Services.Interfaces;

using Ganss.Xss;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class ProfileController(
    HtmlSanitizer sanitizer,
    IEmailService emailService,
    IChangeEmailRepository changeEmailRepository,
    HostConfig hostConfig,
    UserManager<Contact> userManager,
    Serilog.ILogger logger
) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        string? requestingUserId = User.GetUserId();

        if (string.IsNullOrEmpty(requestingUserId))
        {
            throw new InvalidOperationException("User ID is not available.");
        }

        var contact = await userManager.FindByIdAsync(requestingUserId);

        if (contact == null)
        {
            return NotFound();
        }

        return Ok(contact);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        string? requestingUserId = User.GetUserId();

        if (string.IsNullOrEmpty(requestingUserId))
        {
            throw new InvalidOperationException("User ID is not available.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        dto.Sanitize(sanitizer);

        var contact = await userManager.FindByIdAsync(requestingUserId);

        if (contact == null)
        {
            return NotFound();
        }

        // Update only the editable fields
        contact.FirstName = dto.FirstName;
        contact.LastName = dto.LastName;
        contact.Title = dto.Title;
        contact.Occupation = dto.Occupation;
        contact.WorkTasks = dto.WorkTasks;
        contact.PrivateAddress = dto.PrivateAddress;
        contact.PrivateZIP = dto.PrivateZIP;
        contact.PrivateCity = dto.PrivateCity;
        contact.PrivatePhone = dto.PrivatePhone;
        contact.CompanyName = dto.CompanyName;
        contact.CompanyAddress = dto.CompanyAddress;
        contact.CompanyZIP = dto.CompanyZIP;
        contact.CompanyCity = dto.CompanyCity;
        contact.CVRNumber = dto.CVRNumber;
        contact.CompanyPhone = dto.CompanyPhone;
        contact.CompanyEmail = dto.CompanyEmail;
        contact.ElTeknikDelivery = dto.ElTeknikDelivery;
        contact.EANNumber = dto.EANNumber;
        contact.PrimarySection = dto.PrimarySection;
        contact.SecondarySection = dto.SecondarySection;
        contact.InvoiceEmail = dto.InvoiceEmail;

        var result = await userManager.UpdateAsync(contact);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to update profile.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        return Ok(contact);
    }

    [HttpPost]
    [Route("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        string? requestingUserId = User.GetUserId();

        if (string.IsNullOrEmpty(requestingUserId))
        {
            throw new InvalidOperationException("User ID is not available.");
        }

        Contact? contact = await userManager.FindByIdAsync(requestingUserId);

        if (contact is null)
        {
            return NotFound("No user found.");
        }

        IdentityResult result = await userManager.ChangePasswordAsync(contact, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to change password.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        return Ok(new { message = "Password changed successfully." });
    }
    [HttpPost]
    [Route("change-email")]
    [Authorize]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
    {
        string? requestingUserEmail = User.GetEmail();
        string? requestingUserId = User.GetUserId();

        if (string.IsNullOrEmpty(requestingUserEmail))
        {
            throw new InvalidOperationException("User email is not available.");
        }

        if (string.IsNullOrEmpty(requestingUserId))
        {
            throw new InvalidOperationException("User ID is not available.");
        }

        dto.Sanitize(sanitizer);

        var contact = await userManager.FindByIdAsync(requestingUserId);

        if (contact == null)
        {
            return NotFound();
        }

        var changeEmail = new ChangeEmail
        {
            ContactId = contact.Id,
            NewEmail = dto.NewEmail,
            RequestedAt = DateTimeOffset.UtcNow
        };

        var newChangeEmail = await changeEmailRepository.CreateAsync(changeEmail);

        string confirmLink = $"{hostConfig.Audience}/confirm-change-email?token={newChangeEmail.Id}";
        string revokeLink = $"{hostConfig.Audience}/revoke-change-email?token={newChangeEmail.Id}";

        var changeEmailRequest = new ChangeEmailRequest(
            OldEmail: requestingUserEmail,
            NewEmail: dto.NewEmail,
            Name: contact.FirstName,
            ConfirmLink: confirmLink,
            RevokeLink: revokeLink
        );

        await emailService.SendChangeEmailAsync(changeEmailRequest);

        return Ok();
    }

    [HttpPost]
    [Route("change-email/confirm/{id}")]
    public async Task<IActionResult> ConfirmChangeEmail([FromRoute] string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest(new { Message = "id cannot be empty" });
        }

        if (!Guid.TryParse(id, out var guid))
        {
            return BadRequest(new { Message = "id is not a valid guid" });
        }

        var changeEmail = await changeEmailRepository.GetByIdAsync(guid);

        if (changeEmail == null)
        {
            logger.Warning("Change email request not found: {Id}", guid);
            return NotFound();
        }

        if (!changeEmail.IsValid)
        {
            logger.Warning("Change email request is not valid: {Id}", guid);
            return BadRequest(new { Message = "change email request is not valid" });
        }

        await changeEmailRepository.SetAsConfirmedAsync(changeEmail.Id);

        Contact? contact = await userManager.FindByIdAsync(changeEmail.ContactId)
            ?? throw new InvalidOperationException("contact not found");

        contact.Email = changeEmail.NewEmail;
        contact.UserName = changeEmail.NewEmail;
        await userManager.UpdateAsync(contact);

        return Ok();
    }

    [HttpPost]
    [Route("change-email/revoke/{id}")]
    public async Task<IActionResult> RevokeChangeEmail([FromRoute] string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest(new { Message = "id cannot be empty" });
        }

        if (!Guid.TryParse(id, out var guid))
        {
            return BadRequest(new { Message = "id is not a valid guid" });
        }

        var changeEmail = await changeEmailRepository.GetByIdAsync(guid);

        if (changeEmail == null)
        {
            return NotFound();
        }

        if (!changeEmail.IsValid)
        {
            return BadRequest(new { Message = "change email request is not valid" });
        }

        await changeEmailRepository.RevokeAsync(changeEmail.Id);

        return Ok();
    }
}
