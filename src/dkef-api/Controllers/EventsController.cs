using AutoMapper;

using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController(IEventsRepository _repository, IMapper _mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMultiple([FromQuery] int take = 10, [FromQuery] int skip = 0)
    {
        if (take > 50) take = 50;
        return Ok(await _repository.GetMultipleAsync(take, skip));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] EventDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var mappedEvent = _mapper.Map<Event>(dto);

        var newEvent = await _repository.CreateAsync(mappedEvent);

        return CreatedAtAction(nameof(Get), new { id = newEvent.Id }, newEvent);
    }

    [HttpGet(Name = nameof(Get))]
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
}