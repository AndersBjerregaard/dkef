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

    public ValueTask SendChangeEmailAsync(ChangeEmailRequest changeEmailRequest)
    {
        ArgumentNullException.ThrowIfNull(changeEmailRequest);

        var changeEmailDto = mapper.Map<ChangeEmailDto>(changeEmailRequest);

        var changeEmailJson = JsonSerializer.Serialize(changeEmailDto);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            $"postmaster@{mailgunConfiguration.Domain}",
            changeEmailRequest.NewEmail,
            "Skift Email Adresse",
            "change-email",
            changeEmailJson
        );

        var oldEmailDto = mapper.Map<OldChangeEmailDto>(changeEmailRequest);

        var oldEmailJson = JsonSerializer.Serialize(oldEmailDto);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            $"postmaster@{mailgunConfiguration.Domain}",
            changeEmailRequest.OldEmail,
            "Ændring af Email Adresse",
            "old-change-email",
            oldEmailJson
        );


        return ValueTask.CompletedTask;
    }

    public ValueTask SendResetPasswordAsync(ResetPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var resetPasswordDto = mapper.Map<ResetPasswordDto>(request);

        var resetPasswordJson = JsonSerializer.Serialize(resetPasswordDto);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            $"postmaster@{mailgunConfiguration.Domain}",
            request.Email,
            "Ændring af Password",
            "reset-password",
            resetPasswordJson
        );

        return ValueTask.CompletedTask;
    }
}
