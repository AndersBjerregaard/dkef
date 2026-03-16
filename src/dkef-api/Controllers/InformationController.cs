using AutoMapper;

using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Services.Interfaces;

using Ganss.Xss;

using Microsoft.AspNetCore.Mvc;

namespace Dkef.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class InformationController(
    HtmlSanitizer sanitizer,
    IMapper mapper,
    IEmailService emailService
) : ControllerBase {

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] InformationDto dto)
    {
        dto.Sanitize(sanitizer);

        InformationMessage informationMessage = mapper.Map<InformationMessage>(dto);

        // Send email - fire and forget
        _ = emailService.SendContactInquiryAsync(informationMessage);

        return Accepted();
    }
}