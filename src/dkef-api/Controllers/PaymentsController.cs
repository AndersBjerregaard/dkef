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

    [HttpPost("nexi/events/{eventId}/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmNexiEventPayment(
        [FromRoute] Guid eventId,
        [FromBody] NexiEventPaymentConfirmDto dto
    )
    {
        string? requestingUserId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(requestingUserId))
        {
            throw new InvalidOperationException("User ID is not available.");
        }

        string paymentId = dto.PaymentId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            return BadRequest(new { message = "PaymentId er paakraevet." });
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

        EventSignUpPayment? paymentRecord = await contentsContext.EventSignUpPayments
            .Where(x => x.EventId == eventId && x.ContactId == requestingUserId && x.PaymentId == paymentId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (paymentRecord is null)
        {
            return NotFound(new { message = "Betalingen blev ikke fundet for eventet." });
        }

        EventSignUp? existingSignUp = await contentsContext.EventSignUps
            .Where(x => x.EventId == eventId && x.ContactId == requestingUserId)
            .FirstOrDefaultAsync();

        if (existingSignUp is not null)
        {
            paymentRecord.Status = PaymentStatus.Completed;
            paymentRecord.UpdatedAt = DateTime.UtcNow;
            await contentsContext.SaveChangesAsync();

            return Ok(new EventSignUpCreateResponseDto
            {
                EventId = existingSignUp.EventId,
                ContactId = existingSignUp.ContactId,
                SignedUpAt = existingSignUp.SignedUpAt
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/payments/{paymentId}");
        request.Headers.TryAddWithoutValidation("Authorization", _nexiCheckoutConfig.SecretKey);
        request.Headers.TryAddWithoutValidation("Checkout-Key", _nexiCheckoutConfig.CheckoutKey);

        var httpClient = httpClientFactory.CreateClient("NexiCheckoutClient");
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.Warning(
                "Nexi retrieve payment failed for event {EventId}, user {UserId}, payment {PaymentId} with status code {StatusCode}. Response: {ResponseContent}",
                eventId,
                requestingUserId,
                paymentId,
                (int)response.StatusCode,
                responseContent
            );

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Kunne ikke verificere betaling hos Nexi." }
            );
        }

        if (!TryReadNexiPaymentVerification(responseContent, out int chargedAmount, out int reservedAmount, out int orderAmount, out string currency))
        {
            logger.Warning(
                "Nexi retrieve payment response manglede felter for event {EventId}, user {UserId}, payment {PaymentId}. Response: {ResponseContent}",
                eventId,
                requestingUserId,
                paymentId,
                responseContent
            );

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Svar fra Nexi kunne ikke valideres." }
            );
        }

        bool amountMatches = orderAmount == paymentRecord.AmountMinor;
        bool currencyMatches = string.Equals(currency, paymentRecord.Currency, StringComparison.OrdinalIgnoreCase);
        bool isPaid = chargedAmount >= paymentRecord.AmountMinor
            || reservedAmount >= paymentRecord.AmountMinor;

        if (!amountMatches || !currencyMatches || !isPaid)
        {
            paymentRecord.Status = PaymentStatus.Failed;
            paymentRecord.UpdatedAt = DateTime.UtcNow;
            await contentsContext.SaveChangesAsync();

            logger.Warning(
                "Nexi payment verification failed for event {EventId}, user {UserId}, payment {PaymentId}. AmountMatches={AmountMatches}, CurrencyMatches={CurrencyMatches}, Charged={ChargedAmount}, Reserved={ReservedAmount}, ExpectedAmount={ExpectedAmount}, ResponseCurrency={ResponseCurrency}, ExpectedCurrency={ExpectedCurrency}",
                eventId,
                requestingUserId,
                paymentId,
                amountMatches,
                currencyMatches,
                chargedAmount,
                reservedAmount,
                paymentRecord.AmountMinor,
                currency,
                paymentRecord.Currency
            );

            return BadRequest(new { message = "Betalingen er ikke gennemfoert endnu." });
        }

        EventSignUp signUp = new()
        {
            EventId = eventId,
            ContactId = requestingUserId,
            SignedUpAt = DateTime.UtcNow
        };

        try
        {
            contentsContext.EventSignUps.Add(signUp);
            paymentRecord.Status = PaymentStatus.Completed;
            paymentRecord.UpdatedAt = DateTime.UtcNow;
            await contentsContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.Warning(
                ex,
                "Duplicate paid event sign-up blocked for event {EventId} and user {UserId}",
                eventId,
                requestingUserId
            );

            EventSignUp? conflictSignUp = await contentsContext.EventSignUps
                .Where(x => x.EventId == eventId && x.ContactId == requestingUserId)
                .FirstOrDefaultAsync();

            if (conflictSignUp is null)
            {
                return Conflict(new { message = "Brugeren er allerede tilmeldt eventet." });
            }

            paymentRecord.Status = PaymentStatus.Completed;
            paymentRecord.UpdatedAt = DateTime.UtcNow;
            await contentsContext.SaveChangesAsync();

            return Ok(new EventSignUpCreateResponseDto
            {
                EventId = conflictSignUp.EventId,
                ContactId = conflictSignUp.ContactId,
                SignedUpAt = conflictSignUp.SignedUpAt
            });
        }

        return Ok(new EventSignUpCreateResponseDto
        {
            EventId = signUp.EventId,
            ContactId = signUp.ContactId,
            SignedUpAt = signUp.SignedUpAt
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

    private static bool TryReadNexiPaymentVerification(
        string responseContent,
        out int chargedAmount,
        out int reservedAmount,
        out int orderAmount,
        out string currency
    )
    {
        chargedAmount = 0;
        reservedAmount = 0;
        orderAmount = 0;
        currency = string.Empty;

        using JsonDocument document = JsonDocument.Parse(responseContent);
        if (!document.RootElement.TryGetProperty("payment", out JsonElement paymentElement))
        {
            return false;
        }

        if (!paymentElement.TryGetProperty("summary", out JsonElement summaryElement))
        {
            return false;
        }

        if (!paymentElement.TryGetProperty("orderDetails", out JsonElement orderDetailsElement))
        {
            return false;
        }

        if (summaryElement.TryGetProperty("chargedAmount", out JsonElement chargedAmountElement)
            && chargedAmountElement.ValueKind is not JsonValueKind.Null
            && !chargedAmountElement.TryGetInt32(out chargedAmount))
        {
            return false;
        }

        if (!summaryElement.TryGetProperty("reservedAmount", out JsonElement reservedAmountElement)
            || !reservedAmountElement.TryGetInt32(out reservedAmount))
        {
            return false;
        }

        if (!orderDetailsElement.TryGetProperty("amount", out JsonElement orderAmountElement)
            || !orderAmountElement.TryGetInt32(out orderAmount))
        {
            return false;
        }

        if (!orderDetailsElement.TryGetProperty("currency", out JsonElement currencyElement))
        {
            return false;
        }

        string? responseCurrency = currencyElement.GetString();
        if (string.IsNullOrWhiteSpace(responseCurrency))
        {
            return false;
        }

        currency = responseCurrency;
        return true;
    }

    private static class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }
}
