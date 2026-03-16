using System.Text.Json;

using AutoMapper;

using Dkef.Configuration;
using Dkef.Contracts.Mailgun;
using Dkef.Domain;
using Dkef.Services.Interfaces;

namespace Dkef.Services;

public sealed class DevelopmentEmailService(
    MailgunConfiguration mailgunConfiguration,
    IMapper mapper,
    Serilog.ILogger logger
) : IEmailService
{
    public ValueTask SendContactInquiryAsync(InformationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var contactInquiryDto = mapper.Map<ContactInquiryDto>(message) ?? throw new InvalidOperationException("Could not serialize contact inquiry");

        var contactInquiryJson = JsonSerializer.Serialize(contactInquiryDto);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            contactInquiryDto.SenderEmail,
            mailgunConfiguration.To,
            "Ny Henvendelse Modtaget",
            "contact-inquiry",
            contactInquiryJson
        );

        return ValueTask.CompletedTask;
    }
}