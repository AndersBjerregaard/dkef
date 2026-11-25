using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Dkef.Controllers;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Dkef.Tests;

// Real loopback HTTP/MVC/auth/multipart parsing; in-memory persistence/storage boundaries.
// Never starts Program.cs, loads application settings or connects to a database/MinIO.
public sealed class SiteImageHttpServer : IAsyncLifetime
{
    public FakeSiteImages Images { get; } = new();
    public HttpClient Client { get; private set; } = null!;
    private WebApplication? app;
    private readonly SymmetricSecurityKey key = new(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection();
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllers().AddApplicationPart(typeof(SiteImagesController).Assembly);
        builder.Services.AddSingleton<ISiteImageRepository>(Images);
        builder.Services.AddSingleton<ISiteImageStorage>(Images);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = "site-image-tests",
                ValidateAudience = true, ValidAudience = "site-image-tests",
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true, IssuerSigningKey = key, RoleClaimType = ClaimTypes.Role
            };
        });
        builder.Services.AddAuthorization();
        app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        Client = new HttpClient
        {
            BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single())
        };
    }

    public string Token(string role = "Admin", bool identity = true)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, "mail@example.dk"), new(ClaimTypes.Role, role) };
        if (identity) claims.Add(new(ClaimTypes.NameIdentifier, "private-identity-id"));
        return new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), Issuer = "site-image-tests", Audience = "site-image-tests",
            Expires = DateTime.UtcNow.AddMinutes(5), SigningCredentials = new(key, SecurityAlgorithms.HmacSha256)
        });
    }

    public async Task<HttpResponseMessage> Put(byte[] bytes, string? token, string slot = "hero", string alt = "Ny elmast", bool chunked = false)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/SiteImages/home/{slot}");
        request.Headers.ExpectContinue = true;
        if (chunked) request.Headers.TransferEncodingChunked = true;
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes), "file", "untrusted.svg"); // Filename/MIME must not decide validation.
        form.Add(new StringContent(alt), "alt");
        request.Content = form;
        return await Client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> Delete(string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, "/SiteImages/home/hero");
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await Client.SendAsync(request);
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (app is not null) await app.DisposeAsync();
    }
}

public sealed class FakeSiteImages : ISiteImageRepository, ISiteImageStorage
{
    public List<string> Operations { get; } = [];
    public SiteImage? Row { get; set; }
    public bool FailUpload { get; set; }
    public bool FailSave { get; set; }
    public bool FailCleanup { get; set; }
    public void Reset() { Row = null; Operations.Clear(); FailUpload = FailSave = FailCleanup = false; }
    public string PublicUrl(string key) => "https://images.example.dk/site-images/" + key;
    public Task<IReadOnlyList<SiteImage>> GetGroupAsync(string group, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<SiteImage>>(Row is { } row && row.Group == group ? [row] : []);
    public Task UploadAsync(string key, Stream content, CancellationToken ct)
    {
        Operations.Add("upload:" + key);
        if (FailUpload) throw new IOException("private-upload-error");
        return Task.CompletedTask;
    }
    public Task<string?> ReplaceAsync(string group, string slot, SiteImage? image, CancellationToken ct)
    {
        Operations.Add("save");
        if (FailSave) throw new IOException("private-database-error");
        var previous = Row?.ObjectKey;
        Row = image;
        return Task.FromResult(previous);
    }
    public Task DeleteAsync(string key)
    {
        Operations.Add("delete:" + key);
        if (FailCleanup) throw new IOException("private-cleanup-error");
        return Task.CompletedTask;
    }
}

public class SiteImageHttpTests(SiteImageHttpServer server) : IClassFixture<SiteImageHttpServer>
{
    [Fact]
    public async Task PublicReadIsAnonymousUncachedAndExposesOnlyUrlAndAlt()
    {
        server.Images.Reset();
        using var save = await server.Put(SiteImageValidatorTests.ImageBytes(), server.Token());
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        using var read = await server.Client.GetAsync("/SiteImages/home");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.True(read.Headers.CacheControl!.NoStore);
        var json = await read.Content.ReadAsStringAsync();
        Assert.DoesNotContain("private-identity-id", json);
        Assert.DoesNotContain("updatedAt", json);
        var group = JsonSerializer.Deserialize<SiteImageGroupResponse>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal("Ny elmast", Assert.Single(group.Overrides).Value.Alt);
        using var unknown = await server.Client.GetAsync("/SiteImages/Home");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Theory]
    [InlineData("anonymous", 401)]
    [InlineData("invalid", 401)]
    [InlineData("Member", 403)]
    [InlineData("Board Member", 403)]
    [InlineData("admin", 403)]
    [InlineData("missing-id", 403)]
    public async Task OnlyAdminsWithIdentityMayUploadOrReset(string role, int status)
    {
        server.Images.Reset();
        var token = role switch { "anonymous" => null, "invalid" => "invalid", "missing-id" => server.Token(identity: false), _ => server.Token(role) };
        using var put = await server.Put([0], token);
        using var delete = await server.Delete(token);
        Assert.Equal(status, (int)put.StatusCode);
        Assert.Equal(status, (int)delete.StatusCode);
        Assert.Empty(server.Images.Operations);
    }

    [Fact]
    public async Task InvalidUploadsAndUnknownSlotsNeverReachStorage()
    {
        server.Images.Reset();
        foreach (var bytes in new[] { Array.Empty<byte>(), new byte[] { 1, 2 }, new byte[SiteImageValidator.MaxFileBytes + 1] })
        {
            using var response = await server.Put(bytes, server.Token());
            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge });
        }
        using var unknown = await server.Put([1], server.Token(), slot: "logos");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        using var badAlt = await server.Put(SiteImageValidatorTests.ImageBytes(), server.Token(), alt: " ");
        Assert.Equal(HttpStatusCode.BadRequest, badAlt.StatusCode);
        Assert.Empty(server.Images.Operations);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ServerRequestLimitRejectsOversizedMultipart(bool chunked)
    {
        server.Images.Reset();
        using var response = await server.Put(new byte[SiteImageValidator.MaxRequestBytes + 1], server.Token(), chunked: chunked);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Empty(server.Images.Operations);
    }

    [Fact]
    public async Task ReplacementIsUploadedThenCommittedThenOldFileIsDeletedAndResetIsIdempotent()
    {
        server.Images.Reset();
        using var first = await server.Put(SiteImageValidatorTests.ImageBytes(), server.Token());
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var original = server.Images.Row!.ObjectKey;
        server.Images.Operations.Clear();
        using var second = await server.Put(SiteImageValidatorTests.ImageBytes(), server.Token(), alt: "<b>Ny</b> mast");
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.NotEqual(original, server.Images.Row!.ObjectKey);
        Assert.Equal("Ny mast", server.Images.Row.Alt);
        Assert.Equal("private-identity-id", server.Images.Row.UpdatedBy);
        Assert.Equal(["upload:" + server.Images.Row.ObjectKey, "save", "delete:" + original], server.Images.Operations);
        using var reset = await server.Delete(server.Token());
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        Assert.Null(server.Images.Row);
        using var repeated = await server.Delete(server.Token());
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
    }

    [Fact]
    public async Task FailuresPreserveCurrentImageAndCleanupFailureDoesNotFailPublication()
    {
        server.Images.Reset();
        using var first = await server.Put(SiteImageValidatorTests.ImageBytes(), server.Token());
        var original = server.Images.Row!.ObjectKey;
        foreach (var failUpload in new[] { true, false })
        {
            server.Images.Operations.Clear();
            server.Images.FailUpload = failUpload;
            server.Images.FailSave = !failUpload;
            using var failed = await server.Put(SiteImageValidatorTests.ImageBytes(), server.Token());
            Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
            Assert.Equal(original, server.Images.Row.ObjectKey);
            Assert.DoesNotContain(server.Images.Operations, operation => operation.StartsWith("delete:"));
            Assert.DoesNotContain("private-", await failed.Content.ReadAsStringAsync());
        }
        server.Images.FailSave = false;
        server.Images.FailCleanup = true;
        using var success = await server.Put(SiteImageValidatorTests.ImageBytes(), server.Token());
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        Assert.NotEqual(original, server.Images.Row!.ObjectKey);
    }
}
