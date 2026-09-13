using System.Net.Http.Json;
using System.Text.Json;

using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Contracts.Nexi;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class PaymentsController(
    IHttpClientFactory httpClientFactory,
    ContentsContext contentsContext,
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
            EventId = Guid.Empty,
            PaymentId = paymentId,
            AmountMinor = payload.Order.Amount,
            Currency = _nexiCheckoutConfig.Currency,
            CheckoutKey = _nexiCheckoutConfig.CheckoutKey,
            CheckoutJsUrl = _nexiCheckoutConfig.CheckoutJsUrl,
            Language = _nexiCheckoutConfig.Language
        });
    }

    [HttpPost("nexi/events/{eventId}/session")]
    [Authorize]
    public async Task<IActionResult> CreateNexiEventSession([FromRoute] Guid eventId)
    {
        string? requestingUserId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(requestingUserId))
        {
            throw new InvalidOperationException("User ID is not available.");
        }

        Event? eventEntity = await contentsContext.Events.FindAsync(eventId);
        if (eventEntity is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        if (eventEntity.DateTime <= now)
        {
            return BadRequest(new { message = "Event er allerede startet eller afsluttet." });
        }

        if (eventEntity.SignUpDeadline.HasValue && now > eventEntity.SignUpDeadline.Value)
        {
            return BadRequest(new { message = "Tilmeldingsfristen er overskredet." });
        }

        int amountMinor = eventEntity.SignUpPriceMinor ?? 0;
        if (amountMinor <= 0)
        {
            return BadRequest(new { message = "Eventet er gratis og kraever ikke betaling." });
        }

        bool alreadySignedUp = await contentsContext.EventSignUps
            .AnyAsync(x => x.EventId == eventId && x.ContactId == requestingUserId);
        if (alreadySignedUp)
        {
            return Conflict(new { message = "Brugeren er allerede tilmeldt eventet." });
        }

        EventSignUpPayment? existingPendingPayment = await contentsContext.EventSignUpPayments
            .Where(x => x.EventId == eventId
                && x.ContactId == requestingUserId
                && x.Status == PaymentStatus.Pending
                && x.AmountMinor == amountMinor
                && x.Currency == _nexiCheckoutConfig.Currency)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (existingPendingPayment is not null)
        {
            return Ok(new NexiCheckoutSessionDto
            {
                EventId = eventId,
                PaymentId = existingPendingPayment.PaymentId,
                AmountMinor = existingPendingPayment.AmountMinor,
                Currency = existingPendingPayment.Currency,
                CheckoutKey = _nexiCheckoutConfig.CheckoutKey,
                CheckoutJsUrl = _nexiCheckoutConfig.CheckoutJsUrl,
                Language = _nexiCheckoutConfig.Language
            });
        }

        string checkoutUrl = $"{hostConfig.Audience}/payment";
        NexiCreatePaymentRequest payload = BuildEventCreatePaymentRequest(eventEntity, amountMinor, checkoutUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/payments")
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.TryAddWithoutValidation("Authorization", _nexiCheckoutConfig.SecretKey);
        request.Headers.TryAddWithoutValidation("Checkout-Key", _nexiCheckoutConfig.CheckoutKey);
        request.Headers.TryAddWithoutValidation("Idempotency-Key", $"event-{eventId:N}-user-{requestingUserId}-amount-{amountMinor}");

        var httpClient = httpClientFactory.CreateClient("NexiCheckoutClient");
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.Warning(
                "Nexi create event payment failed for event {EventId} and user {UserId} with status code {StatusCode}. Response: {ResponseContent}",
                eventId,
                requestingUserId,
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
            logger.Warning(
                "Nexi event response mangler paymentId for event {EventId} and user {UserId}. Response: {ResponseContent}",
                eventId,
                requestingUserId,
                responseContent
            );

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Svar fra Nexi indeholdt ikke et paymentId." }
            );
        }

        string? paymentId = paymentIdElement.GetString();
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            logger.Warning(
                "Nexi event response har tomt paymentId for event {EventId} and user {UserId}. Response: {ResponseContent}",
                eventId,
                requestingUserId,
                responseContent
            );

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Svar fra Nexi indeholdt et ugyldigt paymentId." }
            );
        }

        EventSignUpPayment eventSignUpPayment = new()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            ContactId = requestingUserId,
            PaymentId = paymentId,
            AmountMinor = amountMinor,
            Currency = _nexiCheckoutConfig.Currency,
            Status = PaymentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        contentsContext.EventSignUpPayments.Add(eventSignUpPayment);
        await contentsContext.SaveChangesAsync();

        return Ok(new NexiCheckoutSessionDto
        {
            EventId = eventId,
            PaymentId = paymentId,
            AmountMinor = amountMinor,
            Currency = _nexiCheckoutConfig.Currency,
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
                UnitPrice = 52500,
                TaxRate = 2500,
                TaxAmount = 17500,
                GrossTotalAmount = 70000,
                NetTotalAmount = 52500
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

    private NexiCreatePaymentRequest BuildEventCreatePaymentRequest(Event eventEntity, int amountMinor, string checkoutUrl)
    {
        var item = new NexiOrderItem
        {
            Reference = $"EVENT-{eventEntity.Id:N}",
            Name = string.IsNullOrWhiteSpace(eventEntity.Title) ? "Event tilmelding" : eventEntity.Title,
            Quantity = 1,
            Unit = "stk",
            UnitPrice = amountMinor,
            TaxRate = 0,
            TaxAmount = 0,
            GrossTotalAmount = amountMinor,
            NetTotalAmount = amountMinor
        };

        return new NexiCreatePaymentRequest
        {
            Order = new NexiOrder
            {
                Items = [item],
                Amount = amountMinor,
                Currency = _nexiCheckoutConfig.Currency,
                Reference = $"DKEF-EVENT-{eventEntity.Id:N}"
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

    private static class PaymentStatus
    {
        public const string Pending = "Pending";
    }
}
