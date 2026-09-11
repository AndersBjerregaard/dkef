using System.Net.Http.Json;
using System.Text.Json;

using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Contracts.Nexi;

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

        var payload = BuildPocCreatePaymentRequest(checkoutUrl);

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

    private NexiCreatePaymentRequest BuildPocCreatePaymentRequest(string checkoutUrl)
    {
        var items = new List<NexiOrderItem>
        {
            new()
            {
                Reference = "POC-MEDLEM-2026",
                Name = "DKEF demo kontingent",
                Quantity = 1,
                Unit = "stk",
                UnitPrice = 75000000,
                TaxRate = 2500,
                TaxAmount = 25000000,
                GrossTotalAmount = 100000000,
                NetTotalAmount = 75000000
            }
        };

        var amount = items.Sum(item => item.GrossTotalAmount);
        if (amount <= 0)
        {
            throw new InvalidOperationException("Nexi order amount must be higher than 0.");
        }

        return new NexiCreatePaymentRequest
        {
            Order = new NexiOrder
            {
                Items = items,
                Amount = amount,
                Currency = _nexiCheckoutConfig.Currency,
                Reference = "DKEF-POC-ORDER"
            },
            Checkout = new NexiCheckout
            {
                IntegrationType = "EmbeddedCheckout",
                Url = checkoutUrl,
                TermsUrl = _nexiCheckoutConfig.TermsUrl,
                MerchantTermsUrl = _nexiCheckoutConfig.MerchantTermsUrl,
                Charge = false
            }
        };
    }
}
