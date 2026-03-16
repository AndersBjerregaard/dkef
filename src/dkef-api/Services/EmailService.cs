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
    IMapper mapper
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
}