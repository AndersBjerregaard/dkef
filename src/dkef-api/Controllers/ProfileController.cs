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
    IContactRepository contactRepository,
    HostConfig hostConfig,
    UserManager<Contact> userManager
) : ControllerBase
{
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

        var contact = await contactRepository.GetByIdAsync(Guid.Parse(requestingUserId));

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

        string confirmLink = $"{hostConfig.UrlPrefix}/profile/change-email/confirm/{newChangeEmail.Id}";
        string revokeLink = $"{hostConfig.UrlPrefix}/profile/change-email/revoke/{newChangeEmail.Id}";

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
            return NotFound();
        }

        if (!changeEmail.IsValid)
        {
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
