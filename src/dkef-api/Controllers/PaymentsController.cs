using System.Net.Http.Json;
using System.Text.Json;

using Dkef.Configuration;
using Dkef.Contracts;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class PaymentsController(
    IHttpClientFactory httpClientFactory,
    IOptions<NexiCheckoutConfig> nexiCheckoutConfigOptions,
    HostConfig hostConfig,
    Serilog.ILogger logger
) : ControllerBase
{
    private readonly NexiCheckoutConfig _nexiCheckoutConfig = nexiCheckoutConfigOptions.Value;

    [HttpPost("nexi/poc-session")]
    public async Task<IActionResult> CreateNexiPocSession()
    {
        var checkoutUrl = $"{hostConfig.Audience}/payment";

        var payload = new
        {
            order = new
            {
                items = new[]
                {
                    new
                    {
                        reference = "POC-MEDLEM-2026",
                        name = "DKEF demo kontingent",
                        quantity = 1,
                        unit = "stk",
                        unitPrice = 100000000,
                        taxRate = 2500,
                        taxAmount = 25000000,
                        grossTotalAmount = 100000000,
                        netTotalAmount = 75000000
                    }
                },
                amount = 100000000,
                currency = _nexiCheckoutConfig.Currency,
                reference = "DKEF-POC-ORDER"
            },
            checkout = new
            {
                integrationType = "EmbeddedCheckout",
                url = checkoutUrl,
                termsUrl = _nexiCheckoutConfig.TermsUrl,
                _nexiCheckoutConfig.MerchantTermsUrl,
                charge = false
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/payments")
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.TryAddWithoutValidation("Authorization", _nexiCheckoutConfig.SecretKey);
        request.Headers.TryAddWithoutValidation("Checkout-Key", _nexiCheckoutConfig.CheckoutKey);
        request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString("N"));

        var httpClient = httpClientFactory.CreateClient("NexiCheckoutClient");
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.Warning(
                "Nexi create payment failed with status code {StatusCode}. Response: {ResponseContent}",
                (int)response.StatusCode,
                responseContent
            );

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Kunne ikke oprette betaling hos Nexi." }
            );
        }

        using var paymentDocument = JsonDocument.Parse(responseContent);
        if (!paymentDocument.RootElement.TryGetProperty("paymentId", out var paymentIdElement))
        {
            logger.Warning("Nexi response mangler paymentId. Response: {ResponseContent}", responseContent);
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Svar fra Nexi indeholdt ikke et paymentId." }
            );
        }

        var paymentId = paymentIdElement.GetString();
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            logger.Warning("Nexi response har tomt paymentId. Response: {ResponseContent}", responseContent);
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Svar fra Nexi indeholdt et ugyldigt paymentId." }
            );
        }

        return Ok(new NexiCheckoutSessionDto
        {
            PaymentId = paymentId,
            CheckoutKey = _nexiCheckoutConfig.CheckoutKey,
            CheckoutJsUrl = _nexiCheckoutConfig.CheckoutJsUrl,
            Language = _nexiCheckoutConfig.Language
        });
    }
}
