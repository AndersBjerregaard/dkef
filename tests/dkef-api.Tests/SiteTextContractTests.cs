using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IdentityModel.Tokens.Jwt;

using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Services;

using Ganss.Xss;

using JsonWebTokenHandler = Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler;
using Microsoft.IdentityModel.Tokens;

using Xunit;

namespace Dkef.Tests;

public class SiteTextContractTests
{
    private static SiteTextPatchValidationResult Validate(string json, string group = "home") =>
        SiteTextPatchContract.Validate(Encoding.UTF8.GetBytes(json), SiteTextRegistryTests.Registry, group);

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{")]
    [InlineData("{\"changes\":{}}")]
    [InlineData("{\"changes\":null}")]
    [InlineData("{\"changes\":[]}")]
    [InlineData("{\"changes\":{},\"extra\":true}")]
    [InlineData("{\"Changes\":{\"hero.title\":\"A\"}}")]
    [InlineData("{\"changes\":{},\"changes\":{\"hero.title\":\"A\"}}")]
    [InlineData("{\"changes\":{\"hero.title\":\"A\",\"hero.title\":\"B\"}}")]
    [InlineData("{\"changes\":{\"hero.title\":\"A\",\"hero.\\u0074itle\":\"B\"}}")]
    [InlineData("{\"changes\":{\"hero.title\":\"\\uD800\"}}")]
    [InlineData("{\"changes\":{\"\\uDC00\":\"A\"}}")]
    [InlineData("{\"\\uDC00\":{\"hero.title\":\"A\"}}")]
    public void MalformedEnvelopesHaveRequestLevelDanishErrors(string json)
    {
        var result = Validate(json);
        Assert.Null(result.Request);
        Assert.Contains("request", result.Errors.Keys);
        Assert.Equal(400, result.ToProblemDetails().Status);
        Assert.Equal("En eller flere tekster er ugyldige.", result.ToProblemDetails().Title);
    }

    [Theory]
    [InlineData("{\"changes\":{\"hero.title\":null}}", "hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":42}}", "hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":true}}", "hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":{}}}", "hero.title")]
    [InlineData("{\"changes\":{\"Hero.title\":\"A\"}}", "Hero.title")]
    [InlineData("{\"changes\":{\"unknown\":\"A\"}}", "unknown")]
    [InlineData("{\"changes\":{\"hero.title\":\"\"}}", "hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":\"<b></b>\"}}", "hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":\"Hej\\nigen\"}}", "hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":\"Hej&#10;igen\"}}", "hero.title")]
    public void InvalidFieldsHaveSemanticPathsAndNoValidRequest(string json, string key)
    {
        var result = Validate(json);
        Assert.Null(result.Request);
        Assert.Contains("changes." + key, result.Errors.Keys);
    }

    [Fact]
    public void InvalidUtf8IsAMalformedRequestNotAnUnexpectedFailure()
    {
        var body = Encoding.UTF8.GetBytes("{\"changes\":{\"hero.title\":\"_").Concat(new byte[] { 0xff })
            .Concat(Encoding.UTF8.GetBytes("\"}}")).ToArray();
        var result = SiteTextPatchContract.Validate(body, SiteTextRegistryTests.Registry, "home");
        Assert.Null(result.Request);
        Assert.Contains("request", result.Errors.Keys);
    }

    [Fact]
    public void MixedBatchNeverReturnsAPartiallyValidRequest()
    {
        var result = Validate("""{"changes":{"hero.title":"Gyldig","hero.description":" "}}""");
        Assert.Null(result.Request);
        Assert.Single(result.Errors);
    }

    [Theory]
    [InlineData("<b>Æble</b> &amp; øl", "Æble & øl")]
    [InlineData("2 < 3 & 4 > 1", "2 < 3 & 4 > 1")]
    [InlineData("&#198; &oslash; &aring;", "Æ ø å")]
    [InlineData("&lt;b&gt;tekst&lt;/b&gt;", "<b>tekst</b>")]
    [InlineData("&amp;lt;", "&lt;")]
    [InlineData("<span onclick='alert(1)'>Hej</span>", "Hej")]
    [InlineData("<script>alert(1)</script>Hej", "alert(1)Hej")]
    [InlineData("  Første\r\n\rAnden  ", "  Første\n\nAnden  ")]
    public void SanitizedResponsesArePlainTextWithoutDoubleEncoding(string raw, string expected)
    {
        var result = Validate(JsonSerializer.Serialize(new { changes = new Dictionary<string, string> { ["hero.description"] = raw } }));
        Assert.Empty(result.Errors);
        Assert.Equal(expected, result.Request!.Changes["hero.description"]);
    }

    [Fact]
    public void RawOversizeCannotBeHiddenByMarkupOrNormalization()
    {
        var raw = "<b>" + new string(' ', 100) + "</b>";
        var result = Validate(JsonSerializer.Serialize(new { changes = new Dictionary<string, string> { ["hero.title"] = raw } }));
        Assert.Null(result.Request);
        Assert.Contains("changes.hero.title", result.Errors.Keys);
    }

    [Fact]
    public void BodyAndChangeCountAreRegistryBounded()
    {
        var registry = SiteTextRegistryTests.Registry;
        var fields = registry.GetFields("home");
        var changes = Enumerable.Range(0, fields.Count + 1).ToDictionary(index => "x" + index, _ => "A");
        Assert.Contains("request", Validate(JsonSerializer.Serialize(new { changes })).Errors.Keys);
        Assert.Contains("request", Validate(new string(' ', (int)SiteTextPatchContract.MaxRequestBytes(registry, "home") + 1)).Errors.Keys);
        Assert.Throws<ArgumentException>(() => Validate("{}", "Home"));
        // Even worst-case escaped registered keys and values fit the published transport bound.
        var all = fields.ToDictionary(pair => pair.Key, pair => new string('\0', pair.Value.MaxLength));
        Assert.True(Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(new { changes = all })) < SiteTextPatchContract.MaxRequestBytes(registry, "home"));
    }

    [Fact]
    public void OptionalEmptyAndDefaultEquivalentValuesRemainExplicitChanges()
    {
        var root = JsonNode.Parse(SiteTextRegistryTests.Json)!;
        root["groups"]![1]!["sections"]![0]!["fields"]![1]!["required"] = false;
        var registry = SiteTextRegistry.Parse(root.ToJsonString());
        var empty = SiteTextPatchContract.Validate(Encoding.UTF8.GetBytes("""{"changes":{"hero.title":""}}"""), registry, "home");
        Assert.Equal("", empty.Request!.Changes["hero.title"]);
        Assert.Single(empty.Request.Changes);
        var sameAsDefault = Validate("""{"changes":{"hero.title":"Dit netværk i elbranchen"}}""");
        Assert.Single(sameAsDefault.Request!.Changes);
        Assert.False(sameAsDefault.Request.Changes.ContainsKey("hero.description"));
    }

    [Fact]
    public void SharedValueExamplesAgreeWithJavaScriptRules()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "site-text-value-cases.json")));
        foreach (var test in document.RootElement.EnumerateArray())
        {
            var field = new SiteTextFieldDefinition("test", "", "Test", 1, test.GetProperty("input").GetString()!,
                test.GetProperty("required").GetBoolean(), test.GetProperty("maxLength").GetInt32(), test.GetProperty("format").GetString()!);
            var value = test.GetProperty("value").GetString()!;
            Assert.Equal(test.GetProperty("normalized").GetString(), SiteTextValueRules.Normalize(value));
            Assert.Equal(test.GetProperty("valid").GetBoolean(), SiteTextValueRules.Validate(field, value) is null);
        }
    }

    [Fact]
    public void ResponsesOmitAuditDataAndEmptyOverridesSerializeAsObject()
    {
        var response = new SiteTextGroupResponse("home", new Dictionary<string, string>());
        Assert.Equal("""{"group":"home","overrides":{}}""", JsonSerializer.Serialize(response));
    }

    [Fact]
    public void FeatureSanitizerDoesNotChangeTheExistingRichTextConfiguration()
    {
        var original = new HtmlSanitizer();
        Assert.Contains("<b>", original.Sanitize("<b>Hej</b>"));
        Assert.Equal("Hej", SiteTextValueRules.Sanitize("<b>Hej</b>", SiteTextValueRules.CreateSanitizer()));
        Assert.Contains("<b>", original.Sanitize("<b>Hej</b>"));
    }

    [Fact]
    public void RevalidatesFormatAfterStrippingMarkupAndDecodingEntities()
    {
        Assert.Equal("info@dkef.dk", Validate("""{"changes":{"secretariat.email":"<b>info@dkef.dk</b>"}}""", "about")
            .Request!.Changes["secretariat.email"]);
        Assert.Null(Validate("""{"changes":{"secretariat.email":"info&#32;@dkef.dk"}}""", "about").Request);
        Assert.Null(Validate("""{"changes":{"secretariat.phone":"&plus; () -"}}""", "about").Request);
    }

    [Fact]
    public void AuditContractUsesTheIdentityIdNotTheEmailSubject()
    {
        // JwtService emits the Identity ID as NameIdentifier AND email as sub. Default inbound mapping
        // maps both to NameIdentifier, so FindFirst(NameIdentifier) is ambiguous and must not be used.
        Assert.Equal(ClaimTypes.NameIdentifier, JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["nameid"]);
        Assert.Equal(ClaimTypes.NameIdentifier, JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["sub"]);
        Assert.Equal("nameid", JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap[ClaimTypes.NameIdentifier]);
    }

    [Fact]
    public async Task AuditIdentitySurvivesActualBearerClaimMappingRegardlessOfClaimOrder()
    {
        var id = Guid.NewGuid().ToString();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("site-text-tests-only-not-a-real-secret-key-123456789"));
        // Same issuer construction as JwtService, with email deliberately first.
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateEncodedJwt(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, "info@example.dk"),
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, "Admin")]),
            Issuer = "site-text-test", Audience = "site-text-test",
            Expires = DateTime.UtcNow.AddMinutes(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        });
        // JwtBearer uses JsonWebTokenHandler, not the legacy handler, by default.
        var validation = await new JsonWebTokenHandler { MapInboundClaims = true }.ValidateTokenAsync(token,
            new TokenValidationParameters
            {
                ValidIssuer = "site-text-test", ValidAudience = "site-text-test", IssuerSigningKey = key
            });
        Assert.True(validation.IsValid);
        var principal = new ClaimsPrincipal(validation.ClaimsIdentity);
        Assert.Equal(2, principal.FindAll(ClaimTypes.NameIdentifier).Count());
        Assert.Equal(id, SiteTextAuditIdentity.FindIdentityId(principal));
        Assert.True(principal.IsInRole("Admin"));
        Assert.Null(SiteTextAuditIdentity.FindIdentityId(new ClaimsPrincipal()));
        Assert.Null(SiteTextAuditIdentity.FindIdentityId(new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "info@example.dk")], "test"))));
        validation.ClaimsIdentity.AddClaim(validation.ClaimsIdentity.Claims.Single(claim =>
            claim.Properties.TryGetValue(JwtSecurityTokenHandler.ShortClaimTypeProperty, out var original)
            && original == "nameid").Clone());
        Assert.Null(SiteTextAuditIdentity.FindIdentityId(principal));
    }
}
