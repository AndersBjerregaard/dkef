using Dkef.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactsController(IContactRepository repository) : ControllerBase {

    [HttpGet]
    public async Task<IActionResult> GetMultiple([FromQuery] int take = 20, [FromQuery] int skip = 0) {
        if (take > 50) take = 50;
        return Ok(await repository.GetMultipleAsync(take, skip));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id) {
        if (!Guid.TryParse(id, out var parsedId)) {
            return BadRequest($"Could not parse {id} as a guid");
        }
        var contact = await repository.GetByIdAsync(parsedId);
        return contact is not null ? Ok(contact) : NotFound();
    }
}