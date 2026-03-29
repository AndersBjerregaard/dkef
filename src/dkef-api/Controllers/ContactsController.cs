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
[Authorize(Roles = "Admin")]
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

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] ContactDto dto)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }

        dto.Sanitize(sanitizer);

        var updatedContact = await repository.UpdateAsync(parsedId, dto);

        return Ok(updatedContact);
    }

    [HttpDelete]
    [Route("{id}")]
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
