using System.Text.Json;

using AutoMapper;

using Dkef.Configuration;
using Dkef.Contracts.Mailgun;
using Dkef.Domain;
using Dkef.Services.Interfaces;

namespace Dkef.Services;

public sealed class EmailService(
    IHttpClientFactory httpClientFactory,
    MailgunConfiguration mailgunConfiguration,
    IMapper mapper,
    ILogger<EmailService> logger
) : IEmailService
{
    public async ValueTask SendContactInquiryAsync(InformationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var contactInquiryDto = mapper.Map<ContactInquiryDto>(message);

        var contactInquiryJson = JsonSerializer.Serialize(contactInquiryDto) ?? throw new InvalidOperationException("Could not serialize contact inquiry");

        HttpClient client = httpClientFactory.CreateClient("Mailgun") ?? throw new InvalidOperationException("Mailgun HttpClient not found");

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.mailgun.net/v3/{mailgunConfiguration.Domain}/messages")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "from", $"{message.Name} <postmaster@{mailgunConfiguration.Domain}>" },
                { "to", mailgunConfiguration.To },
                { "subject", "Ny Henvendelse Modtaget" },
                { "template", "contact-inquiry" },
                { "t:variables", contactInquiryJson }
            })
        };

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async ValueTask SendChangeEmailAsync(ChangeEmailRequest changeEmailRequest)
    {
        ArgumentNullException.ThrowIfNull(changeEmailRequest);

        // New email
        var changeEmailDto = mapper.Map<ChangeEmailDto>(changeEmailRequest);

        var changeEmailJson = JsonSerializer.Serialize(changeEmailDto) ?? throw new InvalidOperationException("Could not serialize change email");

        HttpClient client = httpClientFactory.CreateClient("Mailgun") ?? throw new InvalidOperationException("Mailgun HttpClient not found");

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.mailgun.net/v3/{mailgunConfiguration.Domain}/messages")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "from", $"Elektroteknisk Forening <postmaster@{mailgunConfiguration.Domain}>" },
                { "to", changeEmailRequest.NewEmail },
                { "subject", "Skift Email Adresse" },
                { "template", "change-email" },
                { "t:variables", changeEmailJson }
            })
        };

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to send change email: {StatusCode} - {Error}", response.StatusCode, errorContent);
        }

        // Old email
        var oldEmailDto = mapper.Map<OldChangeEmailDto>(changeEmailRequest);

        var oldEmailJson = JsonSerializer.Serialize(oldEmailDto) ?? throw new InvalidOperationException("Could not serialize old email");

        request = new HttpRequestMessage(HttpMethod.Post, $"https://api.mailgun.net/v3/{mailgunConfiguration.Domain}/messages")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "from", $"Elektroteknisk Forening <postmaster@{mailgunConfiguration.Domain}>" },
                { "to", changeEmailRequest.OldEmail },
                { "subject", "Ændring af Email Adresse" },
                { "template", "old-change-email" },
                { "t:variables", oldEmailJson }
            })
        };

        response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to send old email change: {StatusCode} - {Error}", response.StatusCode, errorContent);
        }
    }
}
