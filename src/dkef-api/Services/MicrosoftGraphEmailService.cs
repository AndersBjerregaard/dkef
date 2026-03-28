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

    public ValueTask SendChangeEmailAsync(ChangeEmailRequest request)
    {
        throw new NotImplementedException();
    }

    public ValueTask SendContactInquiryAsync(InformationMessage message)
    {
        throw new NotImplementedException();
    }

    public async ValueTask SendNewMemberRegisteredAsync(NewMemberRegistered request)
    {
        ArgumentNullException.ThrowIfNull(request);

        logger.Information("Rendering 'NewMemberRegistered' razor template...");
        string rawHtml = await razorRenderer.RenderAsync<NewMember>(new Dictionary<string, object?>
        {
            { "FullName", request.FullName },
            { "Email", request.Email },
            { "Phone", request.Phone },
            { "ReceivedAt", DateTime.UtcNow.ToString("f") }
        });

        logger.Information("Inlining css...");
        InlineResult inlineResult = PreMailer.Net.PreMailer.MoveCssInline(rawHtml);

        foreach (string warning in inlineResult.Warnings)
        {
            logger.Warning(warning);
        }

        string resultHtml = inlineResult.Html;

        var message = new Message
        {
            Subject = "Nyt medlem registreret",
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = resultHtml
            },
            ToRecipients = [
                new Recipient { EmailAddress = new EmailAddress { Address = _mailConfiguration.To }}
            ]
        };

        var mailRequest = new SendMailPostRequestBody
        {
            Message = message
        };

        logger.Information("Sending email...");
        await graphServiceClient.Users[_mailConfiguration.Sender].SendMail.PostAsync(mailRequest);
    }

    public ValueTask SendResetPasswordAsync(ResetPasswordRequest request)
    {
        throw new NotImplementedException();
    }
}
