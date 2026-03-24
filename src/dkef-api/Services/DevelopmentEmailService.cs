using System.Text.Json;

using Dkef.Configuration;
using Dkef.Domain;
using Dkef.Services.Interfaces;

using Microsoft.Extensions.Options;

namespace Dkef.Services;

public sealed class DevelopmentEmailService(
    IOptions<MailConfiguration> mailConfigurationOptions,
    Serilog.ILogger logger
) : IEmailService
{
    private readonly MailConfiguration _mailConfiguration = mailConfigurationOptions.Value;

    public ValueTask SendContactInquiryAsync(InformationMessage informationMessage)
    {
        ArgumentNullException.ThrowIfNull(informationMessage);

        var informationMessageJson = JsonSerializer.Serialize(informationMessage);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            informationMessage.Email,
            _mailConfiguration.To,
            "Ny Henvendelse Modtaget",
            "contact-inquiry",
            informationMessageJson
        );

        return ValueTask.CompletedTask;
    }

    public ValueTask SendChangeEmailAsync(ChangeEmailRequest changeEmailRequest)
    {
        ArgumentNullException.ThrowIfNull(changeEmailRequest);

        var changeEmailJson = JsonSerializer.Serialize(changeEmailRequest);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            $"postmaster@{_mailConfiguration.Domain}",
            changeEmailRequest.NewEmail,
            "Skift Email Adresse",
            "change-email",
            changeEmailJson
        );

        var oldEmailJson = JsonSerializer.Serialize(changeEmailJson);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            $"postmaster@{_mailConfiguration.Domain}",
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

        var resetPasswordJson = JsonSerializer.Serialize(request);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            $"postmaster@{_mailConfiguration.Domain}",
            request.Email,
            "Ændring af Password",
            "reset-password",
            resetPasswordJson
        );

        return ValueTask.CompletedTask;
    }

    public ValueTask SendNewMemberRegisteredAsync(NewMemberRegistered request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var newMemberJson = JsonSerializer.Serialize(request);

        logger.Information("Email Sent!\nFrom: {0}\nTo: {1}\nSubject: {2}\nTemplate {3}\nVariables: {4}",
            $"postmaster@{_mailConfiguration.Domain}",
            _mailConfiguration.To,
            "Ny medlem registreret",
            "new-member",
            newMemberJson
        );

        return ValueTask.CompletedTask;
    }
}
