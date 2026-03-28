using Dkef.Assets.RazorTemplates;
using Dkef.Configuration;
using Dkef.Domain;
using Dkef.Services.Interfaces;

using Microsoft.Extensions.Options;

using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

using PreMailer.Net;

using ILogger = Serilog.ILogger;

namespace Dkef.Services;

public sealed class MicrosoftGraphEmailService(
    IOptions<MailConfiguration> mailConfigurationOptions,
    IRazorRenderer razorRenderer,
    GraphServiceClient graphServiceClient,
    ILogger logger
) : IEmailService
{
    private readonly MailConfiguration _mailConfiguration = mailConfigurationOptions.Value;

    public async Task SendChangeEmailAsync(ChangeEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string receivedAt = DateTime.UtcNow.ToString("f");

        Dictionary<string, object?> newEmailParameters = new()
        {
            { "Name", request.Name },
            { "ConfirmLink", request.ConfirmLink },
            { "ReceivedAt", receivedAt }
        };

        string newEmailRawHtml = await razorRenderer.RenderAsync<Dkef.Assets.RazorTemplates.ChangeEmail>(newEmailParameters);
        string newEmailResultHtml = InlineCss(newEmailRawHtml);

        Dictionary<string, object?> oldEmailParameters = new()
        {
            { "Name", request.Name },
            { "NewEmail", request.NewEmail },
            { "RevokeLink", request.RevokeLink },
            { "ReceivedAt", receivedAt }
        };

        string oldEmailRawHtml = await razorRenderer.RenderAsync<Dkef.Assets.RazorTemplates.OldChangeEmail>(oldEmailParameters);
        string oldEmailResulthtml = InlineCss(oldEmailRawHtml);

        try
        {
            await SendEmailAsync(newEmailResultHtml, "Bekræft ny e-mail", request.NewEmail);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to send change email.");
        }

        try
        {
            await SendEmailAsync(oldEmailResulthtml, "Ændring af e-mail", request.OldEmail);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to send old change email.");
        }
    }

    public async Task SendContactInquiryAsync(InformationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Dictionary<string, object?> parameters = new()
        {
            { "SenderName", message.Name },
            { "SenderEmail", message.Email},
            { "SenderPhone", message.Phone},
            { "Message", message.Message },
            { "ReceivedAt", DateTime.UtcNow.ToString("f") }
        };
        string rawHtml = await razorRenderer.RenderAsync<ContactInquiry>(parameters);

        string resultHtml = InlineCss(rawHtml);

        try
        {
            await SendEmailAsync(resultHtml, "Ny henvendelse", _mailConfiguration.To);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to send contact inquiry email.");
        }
    }

    public async Task SendNewMemberRegisteredAsync(NewMemberRegistered request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Dictionary<string, object?> parameters = new()
        {
            { "FullName", request.FullName },
            { "Email", request.Email },
            { "Phone", request.Phone },
            { "ReceivedAt", DateTime.UtcNow.ToString("f") }
        };
        string rawHtml = await razorRenderer.RenderAsync<NewMember>(parameters);

        string resultHtml = InlineCss(rawHtml);

        try
        {
            await SendEmailAsync(resultHtml, "Nyt medlem registreret", _mailConfiguration.To);
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Failed to send email.");
        }
    }

    public async Task SendResetPasswordAsync(ResetPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Dictionary<string, object?> parameters = new()
        {
            { "Name", request.Name },
            { "ChangeLink", request.ChangeLink },
            { "ReceivedAt", DateTime.UtcNow.ToString("f") }
        };

        string rawHtml = await razorRenderer.RenderAsync<ResetPassword>(parameters);

        string resultHtml = InlineCss(rawHtml);

        try
        {
            await SendEmailAsync(resultHtml, "Nyt medlem registreret", request.Email);
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Failed to send reset password email.");
        }
    }

    private string InlineCss(string rawHtml)
    {

        PreMailer.Net.PreMailer preMailer = new(rawHtml);

        InlineResult inlineResult = preMailer.MoveCssInline();

        foreach (string warning in inlineResult.Warnings)
        {
            logger.Warning(warning);
        }

        return inlineResult.Html;
    }

    private async Task SendEmailAsync(string resultHtml, string subject, string toEmail)
    {
        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = resultHtml
            },
            ToRecipients = [
                new Recipient { EmailAddress = new EmailAddress { Address = toEmail }}
            ]
        };

        var mailRequest = new SendMailPostRequestBody
        {
            Message = message
        };

        logger.Information("Sending email with subject {0} to {1}", subject, toEmail);
        await graphServiceClient.Users[_mailConfiguration.Sender].SendMail.PostAsync(mailRequest);
    }
}
