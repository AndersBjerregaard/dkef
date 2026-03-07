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
[Route("general-assemblies")]
public class GeneralAssembliesController(IGeneralAssemblyRepository _repository, IMapper _mapper, HtmlSanitizer _sanitizer, QueryableService<GeneralAssembly> _queryableService) : ControllerBase
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
    public async Task<IActionResult> Post([FromBody] GeneralAssemblyDto dto)
    {
        dto.Sanitize(_sanitizer);

        var mappedAssembly = _mapper.Map<GeneralAssembly>(dto);

        mappedAssembly.CreatedAt = DateTime.UtcNow;

        var newAssembly = await _repository.CreateAsync(mappedAssembly);

        return CreatedAtAction(nameof(Get), new { id = newAssembly.Id }, newAssembly);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return BadRequest($"Could not parse {id} as a guid");
        }
        var existingAssembly = await _repository.GetByIdAsync(parsedId);
        return existingAssembly is not null ? Ok(existingAssembly) : NotFound();
    }
}
