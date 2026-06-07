using System.Security.Cryptography;

using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;

using Ganss.Xss;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using ILogger = Serilog.ILogger;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactsController(
    IContactRepository repository,
    UserManager<Contact> userManager,
    HtmlSanitizer sanitizer,
    ILogger logger) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetMultiple(
        [FromQuery] uint take = 20,
        [FromQuery] uint skip = 0,
        [FromQuery] uint? memberType = null)
    {
        if (take > 200) take = 200;

        if (memberType.HasValue &&
            Enum.TryParse(memberType.ToString(), out MemberType parsedMemberType))
        {
            return Ok(await repository.GetMultipleListAsync(take, skip, parsedMemberType));
        }

        return Ok(await repository.GetMultipleListAsync(take, skip));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        var contact = await repository.GetByIdAsync(parsedId);
        return contact is not null ? Ok(contact) : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        dto.Sanitize(sanitizer);

        Contact? existing = await userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
        {
            return BadRequest("A user with this email already exists.");
        }

        var contact = new Contact
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name,
            PrimarySection = dto.PrimarySection,
            SecondarySection = dto.SecondarySection,
            Address = dto.Address,
            City = dto.City,
            ZIP = dto.ZIP,
            CountryCode = dto.CountryCode,
            CVRNumber = dto.CVRNumber,
            EANNumber = dto.EANNumber,
            PrivatePhoneNumber = dto.PrivatePhoneNumber,
            AttPerson = dto.AttPerson,
            Title = dto.Title,
            EmploymentStatus = dto.EmploymentStatus,
            Subscription = dto.Subscription,
            CompanyName = dto.CompanyName,
            CompanyAddress = dto.CompanyAddress,
            CompanyZIP = dto.CompanyZip,
            CompanyCity = dto.CompanyCity,
            CompanyPhone = dto.CompanyPhone,
            MagazineDelivery = dto.MagazineDelivery,
            EnrollmentDate = DateTime.UtcNow,
            MemberType = dto.MemberType,
            EmailConfirmed = true, // skip email confirmation — admin-created accounts
        };

        // Generate a cryptographically random temporary password
        var randomBytes = new byte[24];
        RandomNumberGenerator.Fill(randomBytes);
        string temporaryPassword = Convert.ToBase64String(randomBytes);

        IdentityResult result = await userManager.CreateAsync(contact, temporaryPassword);

        if (!result.Succeeded)
        {
            logger.Warning("Admin-initiated member creation failed for {Email}: {Errors}",
                dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

            return BadRequest(new
            {
                message = "Failed to create user.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        logger.Information("Admin created new member {Email}", dto.Email);

        var created = await repository.GetByEmailAsync(dto.Email);
        return CreatedAtAction(nameof(Get), new { id = created!.Id }, created);
    }

    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] ContactDto dto)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }

        dto.Sanitize(sanitizer);

        var contact = await userManager.FindByIdAsync(id);

        if (contact is null) {
            return NotFound();
        }

        // Update fields
        contact.Email = dto.Email;
        contact.UserName = dto.Email;
        contact.Address = dto.Address;
        contact.City = dto.City;
        contact.ZIP = dto.ZIP;
        contact.CountryCode = dto.CountryCode;
        contact.CVRNumber = dto.CVRNumber;
        contact.EANNumber = dto.EANNumber;
        contact.PrivatePhoneNumber = dto.PrivatePhoneNumber;
        contact.AttPerson = dto.AttPerson;
        contact.Subscription = dto.Subscription;
        contact.CompanyName = dto.CompanyName;
        contact.CompanyZIP = dto.CompanyZIP;
        contact.CompanyCity = dto.CompanyCity;
        contact.CompanyPhone = dto.CompanyPhone;
        contact.EmploymentStatus = dto.EmploymentStatus;
        contact.PrimarySection = dto.PrimarySection;
        contact.SecondarySection = dto.SecondarySection;
        contact.MagazineDelivery = dto.MagazineDelivery;
        contact.Title = dto.Title;
        contact.MemberType = dto.MemberType;

        var identityResult = await userManager.UpdateAsync(contact);

        // Apply roles
        if (dto.MemberType == MemberType.Member) {
            await userManager.RemoveFromRolesAsync(
                contact,
                [
                    Dkef.Domain.Roles.BoardMember,
                    Dkef.Domain.Roles.Admin
                ]
            );
        }
        if (dto.MemberType == MemberType.BoardMember) {
            await userManager.RemoveFromRoleAsync(contact, Dkef.Domain.Roles.Admin);
            await userManager.AddToRoleAsync(contact, Dkef.Domain.Roles.BoardMember);
        }
        if (dto.MemberType == MemberType.Admin) {
            await userManager.RemoveFromRoleAsync(contact, Dkef.Domain.Roles.BoardMember);
            await userManager.AddToRoleAsync(contact, Dkef.Domain.Roles.Admin);
        }

        if (!identityResult.Succeeded) {
            return BadRequest(string.Join(", ", identityResult.Errors.Select(x => x.Description)));
        }

        return Ok(contact);
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }

        var contact = await userManager.FindByIdAsync(id.ToString());

        if (contact is null)
        {
            return NotFound();
        }

        await userManager.DeleteAsync(contact);

        return NoContent();
    }

    [HttpGet]
    [Route("{id}/authorize")]
    [Authorize(Roles = "Admin")]
    public IActionResult AuthorizeEdit([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out _))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        return Ok();
    }

#if DEBUG
    // Endpoint only available in development
    [HttpPost]
    [Route("seed")]
    public async Task<IActionResult> Seed()
    {
        logger.Information("Seeding...");

        await repository.SeedAsync();

        return Ok();
    }
#endif
}