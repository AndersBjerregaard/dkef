using AutoMapper;

using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Domain.Abstracts;
using Dkef.Repositories;
using Dkef.Services;

using Ganss.Xss;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class ContentsController(
    ContentRepository repository,
    QueryableService<BaseContent> contentQueryableService,
    QueryableService<Event> eventQueryableService,
    QueryableService<News> newsQueryableService,
    QueryableService<GeneralAssembly> generalAssemblyQueryableService,
    HtmlSanitizer htmlSanitizer,
    IMapper mapper,
    Serilog.ILogger logger
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMultipleContents(
        [FromQuery] int take = 10,
        [FromQuery] int skip = 0,
        [FromQuery] string orderBy = "Id",
        [FromQuery] string sortOrder = "asc"
    )
    {
        if (take > 50)
        {
            logger.Warning("Take parameter exceeds 50, setting to 50");
            take = 50;
        }

        IOrderedQueryable<BaseContent> orderExpression = contentQueryableService
            .GetQuery(orderBy, sortOrder);

        DomainCollection<BaseContent> result = await repository.GetMultiple(orderExpression, take, skip);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContent([FromRoute] Guid id)
    {
        var content = await repository.GetByIdAsync<BaseContent>(id);

        if (content == null)
        {
            return NotFound();
        }

        await repository.DeleteAsync(content);

        return NoContent();
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetMultipleEvents(
        [FromQuery] int take = 10,
        [FromQuery] int skip = 0,
        [FromQuery] string orderBy = "Id",
        [FromQuery] string sortOrder = "asc"
    )
    {
        if (take > 50)
        {
            logger.Warning("Take parameter exceeds 50, setting to 50");
            take = 50;
        }

        IOrderedQueryable<Event> orderExpression = eventQueryableService
            .GetQuery(orderBy, sortOrder);

        DomainCollection<Event> result = await repository.GetMultiple(orderExpression, take, skip);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent([FromBody] EventDto dto)
    {
        dto.Sanitize(htmlSanitizer);

        Event eventEntity = mapper.Map<Event>(dto);

        await repository.CreateAsync(eventEntity);

        return CreatedAtAction(nameof(GetEventById), new { id = eventEntity.Id }, eventEntity);
    }

    [HttpGet("events/{id}")]
    public async Task<IActionResult> GetEventById([FromRoute] Guid id)
    {
        Event? eventEntity = await repository.GetByIdAsync<Event>(id);

        if (eventEntity is null)
        {
            return NotFound();
        }

        return Ok(eventEntity);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("events/{id}")]
    public async Task<IActionResult> UpdateEvent([FromRoute] Guid id, [FromBody] EventDto dto)
    {
        dto.Sanitize(htmlSanitizer);

        Event? eventEntity = await repository.GetByIdAsync<Event>(id);

        if (eventEntity is null)
        {
            return NotFound();
        }

        mapper.Map(dto, eventEntity);

        await repository.UpdateAsync(eventEntity);

        return Ok(eventEntity);
    }

    [HttpGet("news")]
    public async Task<IActionResult> GetMultipleNews(
        [FromQuery] int take = 10,
        [FromQuery] int skip = 0,
        [FromQuery] string orderBy = "Id",
        [FromQuery] string sortOrder = "asc"
    )
    {
        if (take > 50)
        {
            logger.Warning("Take parameter exceeds 50, setting to 50");
            take = 50;
        }

        IOrderedQueryable<News> orderExpression = newsQueryableService
            .GetQuery(orderBy, sortOrder);

        DomainCollection<News> result = await repository.GetMultiple(orderExpression, take, skip);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("news")]
    public async Task<IActionResult> CreateNews([FromBody] NewsDto dto)
    {
        dto.Sanitize(htmlSanitizer);

        News newsEntity = mapper.Map<News>(dto);

        await repository.CreateAsync(newsEntity);

        return CreatedAtAction(nameof(GetNewsById), new { id = newsEntity.Id }, newsEntity);
    }

    [HttpGet("news/{id}")]
    public async Task<IActionResult> GetNewsById([FromRoute] Guid id)
    {
        News? newsEntity = await repository.GetByIdAsync<News>(id);

        if (newsEntity is null)
        {
            return NotFound();
        }

        return Ok(newsEntity);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("news/{id}")]
    public async Task<IActionResult> UpdateNews([FromRoute] Guid id, [FromBody] NewsDto dto)
    {
        dto.Sanitize(htmlSanitizer);

        News? newsEntity = await repository.GetByIdAsync<News>(id);

        if (newsEntity is null)
        {
            return NotFound();
        }

        mapper.Map(dto, newsEntity);

        await repository.UpdateAsync(newsEntity);

        return Ok(newsEntity);
    }

    [HttpGet("general-assemblies")]
    public async Task<IActionResult> GetMultipleGeneralAssemblies(
        [FromQuery] int take = 10,
        [FromQuery] int skip = 0,
        [FromQuery] string orderBy = "Id",
        [FromQuery] string sortOrder = "asc"
    )
    {
        if (take > 50)
        {
            logger.Warning("Take parameter exceeds 50, setting to 50");
            take = 50;
        }

        IOrderedQueryable<GeneralAssembly> orderExpression = generalAssemblyQueryableService
            .GetQuery(orderBy, sortOrder);

        DomainCollection<GeneralAssembly> result = await repository.GetMultiple(orderExpression, take, skip);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("general-assemblies")]
    public async Task<IActionResult> CreateGeneralAssembly([FromBody] GeneralAssemblyDto dto)
    {
        dto.Sanitize(htmlSanitizer);

        GeneralAssembly generalAssemblyEntity = mapper.Map<GeneralAssembly>(dto);

        await repository.CreateAsync(generalAssemblyEntity);

        return CreatedAtAction(nameof(GetGeneralAssemblyById), new { id = generalAssemblyEntity.Id }, generalAssemblyEntity);
    }

    [HttpGet("general-assemblies/{id}")]
    public async Task<IActionResult> GetGeneralAssemblyById([FromRoute] Guid id)
    {
        GeneralAssembly? generalAssemblyEntity = await repository.GetByIdAsync<GeneralAssembly>(id);

        if (generalAssemblyEntity is null)
        {
            return NotFound();
        }

        return Ok(generalAssemblyEntity);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("general-assemblies/{id}")]
    public async Task<IActionResult> UpdateGeneralAssembly([FromRoute] Guid id, [FromBody] GeneralAssemblyDto dto)
    {
        dto.Sanitize(htmlSanitizer);

        GeneralAssembly? generalAssemblyEntity = await repository.GetByIdAsync<GeneralAssembly>(id);

        if (generalAssemblyEntity is null)
        {
            return NotFound();
        }

        mapper.Map(dto, generalAssemblyEntity);

        await repository.UpdateAsync(generalAssemblyEntity);

        return Ok(generalAssemblyEntity);
    }
}
