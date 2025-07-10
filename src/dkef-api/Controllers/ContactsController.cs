using Dkef.Contracts;
using Dkef.Repositories;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactsController(IContactRepository _repository, HtmlSanitizer _sanitizer, ILogger _logger) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetMultiple([FromQuery] int take = 20, [FromQuery] int skip = 0)
    {
        if (take > 50) take = 50;
        return Ok(await _repository.GetMultipleAsync(take, skip));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        var contact = await _repository.GetByIdAsync(parsedId);
        return contact is not null ? Ok(contact) : NotFound();
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] ContactDto dto)
    {
        // TODO: Auth

        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        dto.Sanitize(_sanitizer);

        // TODO: System properties, logs...

        var updatedContact = await _repository.UpdateAsync(parsedId, dto);

        return Ok(updatedContact);
    }

    [HttpGet]
    [Route("{id}/authorize")]
    public IActionResult AuthorizeEdit([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out _))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        // TODO: Do some auth
        return Ok();
    }

// Endpoint only available in development
#if DEBUG
    [HttpPost]
    [Route("seed")]
    public async Task<IActionResult> Seed()
    {
        _logger.Information("Seeding...");

        await _repository.SeedAsync();

        return Ok();
    }
#endif
}