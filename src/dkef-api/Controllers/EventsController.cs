using AutoMapper;
using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController(IEventsRepository _repository, IMapper _mapper, HtmlSanitizer _sanitizer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMultiple([FromQuery] int take = 10, [FromQuery] int skip = 0, [FromQuery] string orderBy = "Id", [FromQuery] string order = "asc")
    {
        if (take > 50) take = 50;

        var orderExpression = SortOrderService.GetKeySelector<Event>(orderBy);

        return Ok(await _repository.GetMultipleAsync(orderExpression,  take, skip));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] EventDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        dto.Sanitize(_sanitizer);

        var mappedEvent = _mapper.Map<Event>(dto);

        mappedEvent.CreatedAt = DateTime.UtcNow;

        var newEvent = await _repository.CreateAsync(mappedEvent);

        return CreatedAtAction(nameof(Get), new { id = newEvent.Id }, newEvent);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        var existingEvent = await _repository.GetByIdAsync(parsedId);
        return existingEvent is not null ? Ok(existingEvent) : NotFound();
    }
}