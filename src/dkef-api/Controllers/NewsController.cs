using AutoMapper;

using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;

using Ganss.Xss;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsController(INewsRepository _repository, IMapper _mapper, HtmlSanitizer _sanitizer, QueryableService<News> _queryableService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMultiple([FromQuery] int take = 10, [FromQuery] int skip = 0, [FromQuery] string orderBy = "Id", [FromQuery] string order = "asc")
    {
        if (take > 50) take = 50;

        var orderExpression = _queryableService.GetQuery(orderBy, order);

        return Ok(await _repository.GetMultipleAsync(orderExpression, take, skip));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] NewsDto dto)
    {
        dto.Sanitize(_sanitizer);

        var mappedNews = _mapper.Map<News>(dto);

        mappedNews.PublishedAt = DateTime.UtcNow;
        mappedNews.CreatedAt = DateTime.UtcNow;

        var newNews = await _repository.CreateAsync(mappedNews);

        return CreatedAtAction(nameof(Get), new { id = newNews.Id }, newNews);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        var existingNews = await _repository.GetByIdAsync(parsedId);
        return existingNews is not null ? Ok(existingNews) : NotFound();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put([FromRoute] string id, [FromBody] NewsDto dto)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }

        dto.Sanitize(_sanitizer);

        try
        {
            var updatedNews = await _repository.UpdateAsync(parsedId, dto);
            return Ok(updatedNews);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }

        try
        {
            await _repository.DeleteAsync(parsedId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
