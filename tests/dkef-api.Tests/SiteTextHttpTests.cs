using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using Dkef.Contracts;
using Dkef.Controllers;
using Dkef.Repositories;
using Dkef.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

using Xunit;

namespace Dkef.Tests;

// Real loopback HTTP, MVC routing/authorization, Bearer claim mapping, and PostgreSQL. No new test
// server package, application startup, shared settings, user seeding, or application database access.
public sealed class SiteTextHttpServer : IAsyncLifetime
{
    public SiteTextPostgresDatabase Database { get; } = new();
    public HttpWriteGate Gate { get; } = new();
    public SafeControllerLog Log { get; } = new();
    public SiteTextRegistry Registry { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    private readonly SymmetricSecurityKey key = new(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    private WebApplication? app;

    public async Task InitializeAsync()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DKEF_TEST_POSTGRES"))) return;
        await Database.InitializeAsync();
        try
        {
            var json = JsonNode.Parse(SiteTextRegistryTests.Json)!;
            // Future optional slot: test-only definition, never a change to the approved inventory.
            var future = json["groups"]![1]!.DeepClone();
            future["id"] = "future";
            future["order"] = 5;
            future["sections"]![0]!["fields"]![1]!["required"] = false;
            json["groups"]!.AsArray().Add(future);
            Registry = SiteTextRegistry.Parse(json.ToJsonString());

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection();
            builder.Logging.ClearProviders();
            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
            builder.Services.AddControllers().AddApplicationPart(typeof(SiteTextsController).Assembly);
            builder.Services.AddSingleton(Registry);
            builder.Services.AddSingleton<ILogger<SiteTextsController>>(Log);
            builder.Services.AddScoped(_ => Database.CreateContext(Gate));
            builder.Services.AddScoped<SiteTextRepository>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                // Match Program.cs validation and the JwtService issuer's nameid/sub/role mapping.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, ValidIssuer = "site-text-http-tests",
                    ValidateAudience = true, ValidAudience = "site-text-http-tests",
                    ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true, IssuerSigningKey = key,
                    RoleClaimType = ClaimTypes.Role
                };
            });
            builder.Services.AddAuthorization();
            app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features
                .Get<IServerAddressesFeature>()!.Addresses.Single();
            Client = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(30) };
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public string Token(string role = "Admin", string? identity = "identity-id", bool expired = false)
    {
        // Email deliberately precedes nameid to expose accidental FindFirst(NameIdentifier) usage.
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, "info@example.dk"), new(ClaimTypes.Role, role) };
        if (identity is not null) claims.Add(new(ClaimTypes.NameIdentifier, identity));
        return new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = "site-text-http-tests", Audience = "site-text-http-tests",
            NotBefore = DateTime.UtcNow.AddMinutes(-10),
            Expires = DateTime.UtcNow.AddMinutes(expired ? -1 : 5),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        });
    }

    public async Task<HttpResponseMessage> Patch(string json, string? token, string group = "home", bool chunked = false)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, "/SiteTexts/" + group);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = chunked ? new UnknownLengthJson(json) : new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.SendAsync(request);
    }

    public async Task Reset()
    {
        await using var context = Database.CreateContext();
        await context.SiteTexts.ExecuteDeleteAsync();
        Log.Messages.Clear();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (app is not null) await app.DisposeAsync();
        await Database.DisposeAsync();
    }

    private sealed class UnknownLengthJson(string json) : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            stream.WriteAsync(Encoding.UTF8.GetBytes(json)).AsTask();
    }
}

public class SiteTextHttpTests(SiteTextHttpServer server) : IClassFixture<SiteTextHttpServer>
{
    private static string Changes(params (string Key, string Value)[] changes) =>
        JsonSerializer.Serialize(new { changes = changes.ToDictionary(pair => pair.Key, pair => pair.Value) });

    private static async Task<SiteTextGroupResponse> Saved(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<SiteTextGroupResponse>())!;
    }

    [PostgresFact]
    public async Task PublicReadsAreAnonymousUncachedAndFilterRetiredKeysAndAuditData()
    {
        await server.Reset();
        using var empty = await server.Client.GetAsync("/SiteTexts/home");
        Assert.True(empty.Headers.CacheControl!.NoStore);
        Assert.Empty((await Saved(empty)).Overrides);
        await using var context = server.Database.CreateContext();
        var repository = new SiteTextRepository(context);
        await repository.SaveBatchAsync("home", new Dictionary<string, string>
        {
            ["hero.title"] = "Offentlig æøå", ["retired.key"] = "Skjult"
        }, "private-audit-id");
        await repository.SaveBatchAsync("about", new Dictionary<string, string> { ["introduction.title"] = "Anden gruppe" }, "private-audit-id");
        using var read = await server.Client.GetAsync("/SiteTexts/home");
        var json = JsonNode.Parse(await read.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(["group", "overrides"], json.Select(pair => pair.Key));
        Assert.Equal("Offentlig æøå", json["overrides"]!["hero.title"]!.GetValue<string>());
        Assert.Single(json["overrides"]!.AsObject());
        foreach (var group in new[] { "unknown", "Home" })
        {
            using var unknown = await server.Client.GetAsync("/SiteTexts/" + group);
            Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        }
        // An invalid token must not make an explicitly anonymous public read require login.
        using var request = new HttpRequestMessage(HttpMethod.Get, "/SiteTexts/home");
        request.Headers.Authorization = new("Bearer", "invalid");
        using var anonymous = await server.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, anonymous.StatusCode);
        using var patch = await server.Patch(Changes(("hero.description", "Ny beskrivelse")), server.Token());
        var saved = await Saved(patch);
        Assert.Equal(2, saved.Overrides.Count);
        Assert.False(saved.Overrides.ContainsKey("retired.key"));
        Assert.Equal("Offentlig æøå", saved.Overrides["hero.title"]);
    }

    [PostgresTheory]
    [InlineData("anonymous", 401)]
    [InlineData("invalid", 401)]
    [InlineData("expired", 401)]
    [InlineData("Member", 403)]
    [InlineData("Board Member", 403)]
    [InlineData("admin", 403)]
    [InlineData("missing-id", 403)]
    public async Task OnlyAnAdminWithAnAuditIdentityMayWrite(string kind, int status)
    {
        await server.Reset();
        var token = kind switch
        {
            "anonymous" => null, "invalid" => "not-a-token", "expired" => server.Token(expired: true),
            "missing-id" => server.Token(identity: null), _ => server.Token(kind)
        };
        // Malformed body proves auth/audit rejection occurs before parsing or persistence.
        using var response = await server.Patch("{", token);
        Assert.Equal(status, (int)response.StatusCode);
        await using var context = server.Database.CreateContext();
        Assert.Empty(await context.SiteTexts.ToListAsync());
    }

    [PostgresFact]
    public async Task AdminSaveReturnsAuthoritativeSanitizedSnapshotAndStableAuditIdentity()
    {
        await server.Reset();
        var identity = Guid.NewGuid().ToString();
        using var first = await server.Patch(Changes(("hero.title", "<b>Æble</b> &amp; øl"),
            ("hero.description", "  Første\r\n\rAnden  ")), server.Token(identity: identity));
        var saved = await Saved(first);
        Assert.Equal("Æble & øl", saved.Overrides["hero.title"]);
        Assert.Equal("  Første\n\nAnden  ", saved.Overrides["hero.description"]);
        using var second = await server.Patch(Changes(("hero.title", "&lt;b&gt;tekst&lt;/b&gt; &amp;lt;")), server.Token());
        saved = await Saved(second);
        Assert.Equal("<b>tekst</b> &lt;", saved.Overrides["hero.title"]); // No second sanitization/decode.
        Assert.Equal("  Første\n\nAnden  ", saved.Overrides["hero.description"]); // Omitted survives.
        await using var context = server.Database.CreateContext();
        Assert.Equal(identity, (await context.SiteTexts.SingleAsync(row => row.Key == "hero.description")).UpdatedBy);
        Assert.Equal(saved.Overrides, (await Saved(await server.Client.GetAsync("/SiteTexts/home"))).Overrides);
    }

    [PostgresTheory]
    [InlineData("<script>alert(1)</script>Hej", "alert(1)Hej")]
    [InlineData("<span onclick='alert(1)'>Hej</span>", "Hej")]
    [InlineData("2 < 3 & 4 > 1", "2 < 3 & 4 > 1")]
    [InlineData("&#198; &oslash; &aring;", "Æ ø å")]
    public async Task MarkupAndEntitiesAreStoredAndReturnedAsInertPlainText(string raw, string expected)
    {
        await server.Reset();
        using var response = await server.Patch(Changes(("hero.description", raw)), server.Token());
        Assert.Equal(expected, (await Saved(response)).Overrides["hero.description"]);
        await using var context = server.Database.CreateContext();
        Assert.Equal(expected, Assert.Single(await context.SiteTexts.ToListAsync()).Value);
    }

    [PostgresTheory]
    [InlineData("{}", "request")]
    [InlineData("{", "request")]
    [InlineData("{\"changes\":{},\"changes\":{\"hero.title\":\"A\"}}", "request")]
    [InlineData("{\"changes\":{\"hero.title\":\"A\",\"hero.\\u0074itle\":\"B\"}}", "request")]
    [InlineData("{\"changes\":{\"hero.title\":\"A\",\"hero.description\":\" \"}}", "changes.hero.description")]
    [InlineData("{\"changes\":{\"unknown\":\"A\"}}", "changes.unknown")]
    [InlineData("{\"changes\":{\"hero.title\":null}}", "changes.hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":42}}", "changes.hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":\"<b></b>\"}}", "changes.hero.title")]
    [InlineData("{\"changes\":{\"hero.title\":\"A&#10;B\"}}", "changes.hero.title")]
    public async Task InvalidRequestsReturnDanishProblemsAndLeaveEveryRowUnchanged(string json, string path)
    {
        await server.Reset();
        using var initial = await server.Patch(Changes(("hero.title", "Original")), server.Token());
        await Saved(initial);
        using var invalid = await server.Patch(json, server.Token());
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType!.MediaType);
        var problem = JsonNode.Parse(await invalid.Content.ReadAsStringAsync())!;
        Assert.Equal("En eller flere tekster er ugyldige.", problem["title"]!.GetValue<string>());
        Assert.NotNull(problem["errors"]![path]);
        await using var context = server.Database.CreateContext();
        Assert.Equal("Original", Assert.Single(await context.SiteTexts.ToListAsync()).Value);
    }

    [PostgresFact]
    public async Task UnknownGroupsEmailAndRawValueBoundsAreEnforced()
    {
        await server.Reset();
        using var unknown = await server.Patch(Changes(("hero.title", "A")), server.Token(), "Home");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        using var email = await server.Patch(Changes(("secretariat.email", "info&#32;@dkef.dk")), server.Token(), "about");
        Assert.Equal(HttpStatusCode.BadRequest, email.StatusCode);
        var max = server.Registry.GetFields("home")["hero.title"].MaxLength;
        var atLimit = string.Concat(Enumerable.Repeat("😀", max / 2)) + (max % 2 == 1 ? "Æ" : "");
        using var boundary = await server.Patch(Changes(("hero.title", atLimit)), server.Token());
        Assert.Equal(max, (await Saved(boundary)).Overrides["hero.title"].Length);
        foreach (var value in new[] { new string('Æ', max + 1), "<b>" + new string('x', max) + "</b>" })
        {
            using var tooLong = await server.Patch(Changes(("hero.title", value)), server.Token());
            Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        }
        await using var context = server.Database.CreateContext();
        Assert.Equal(atLimit, Assert.Single(await context.SiteTexts.ToListAsync()).Value);
    }

    [PostgresFact]
    public async Task TransportBoundIsInclusiveAndAppliesToChunkedBodies()
    {
        await server.Reset();
        var maximum = (int)SiteTextPatchContract.MaxRequestBytes(server.Registry, "home");
        var json = Changes(("hero.title", "A"));
        using var atBound = await server.Patch(json.PadRight(maximum), server.Token(), chunked: true);
        await Saved(atBound);
        foreach (var chunked in new[] { false, true })
        {
            using var tooLarge = await server.Patch(json.PadRight(maximum + 1), server.Token(), chunked: chunked);
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, tooLarge.StatusCode);
            var problem = JsonNode.Parse(await tooLarge.Content.ReadAsStringAsync())!;
            Assert.Equal("Anmodningen er for stor.", problem["errors"]!["request"]![0]!.GetValue<string>());
        }
        await using var context = server.Database.CreateContext();
        Assert.Equal("A", Assert.Single(await context.SiteTexts.ToListAsync()).Value);
    }

    [PostgresFact]
    public async Task OptionalEmptyAndDefaultEquivalentOverridesAreNotDeletes()
    {
        await server.Reset();
        using var optional = await server.Patch(Changes(("hero.title", "")), server.Token(), "future");
        Assert.Equal("", (await Saved(optional)).Overrides["hero.title"]);
        var wording = server.Registry.GetFields("home")["hero.title"].Default;
        using var same = await server.Patch(Changes(("hero.title", wording)), server.Token());
        Assert.Equal(wording, (await Saved(same)).Overrides["hero.title"]);
        await using var context = server.Database.CreateContext();
        Assert.Equal(2, await context.SiteTexts.CountAsync());
        using var read = await server.Client.GetAsync("/SiteTexts/future");
        Assert.Equal("", (await Saved(read)).Overrides["hero.title"]);
    }

    [PostgresFact]
    public async Task PersistenceFailureIsGenericSafelyLoggedAndRollsBackTheWholeBatch()
    {
        await server.Reset();
        using var original = await server.Patch(Changes(("hero.description", "Original")), server.Token());
        await Saved(original);
        await using var context = server.Database.CreateContext();
        await context.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "SiteTexts" ADD CONSTRAINT "HttpTestRejectedValue" CHECK ("Value" <> 'private-rejected-value')
            """);
        try
        {
            using var failed = await server.Patch(Changes(("hero.description", "Must roll back"),
                ("hero.title", "private-rejected-value")), server.Token(identity: "private-audit-id"));
            Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
            var body = await failed.Content.ReadAsStringAsync();
            Assert.DoesNotContain("private-", body);
            Assert.DoesNotContain("INSERT", body);
            Assert.Equal("Original", Assert.Single(await context.SiteTexts.AsNoTracking().ToListAsync()).Value);
            var log = Assert.Single(server.Log.Messages);
            Assert.Contains("PostgresException", log);
            Assert.DoesNotContain("private-", log);
            Assert.DoesNotContain("INSERT", log);
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"SiteTexts\" DROP CONSTRAINT \"HttpTestRejectedValue\"");
        }
    }

    [PostgresFact]
    public async Task PublicReadFailureIsGenericAndStillUncached()
    {
        await using var context = server.Database.CreateContext();
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"SiteTexts\" RENAME TO \"HiddenSiteTexts\"");
        try
        {
            using var failed = await server.Client.GetAsync("/SiteTexts/home");
            Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
            Assert.True(failed.Headers.CacheControl!.NoStore);
            Assert.DoesNotContain("SiteTexts", await failed.Content.ReadAsStringAsync());
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"HiddenSiteTexts\" RENAME TO \"SiteTexts\"");
        }
    }

    [PostgresTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ConcurrentAdministratorsPreserveDisjointChangesAndLastCommitWins(bool existing, bool disjoint)
    {
        await server.Reset();
        if (existing) await Saved(await server.Patch(Changes(("hero.title", "Before")), server.Token()));
        var firstId = Guid.NewGuid().ToString();
        var secondId = Guid.NewGuid().ToString();
        server.Gate.Arm(firstId, secondId);
        var first = server.Patch(Changes(("hero.title", "First")), server.Token(identity: firstId));
        Task<HttpResponseMessage>? second = null;
        try
        {
            var firstPid = await server.Gate.Written.Task.WaitAsync(TimeSpan.FromSeconds(10));
            second = server.Patch(Changes((disjoint ? "hero.description" : "hero.title", "Second")), server.Token(identity: secondId));
            var secondPid = await server.Gate.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            if (disjoint) Assert.Equal(HttpStatusCode.OK, (await second.WaitAsync(TimeSpan.FromSeconds(10))).StatusCode);
            else
            {
                await using var reader = server.Database.CreateContext();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                while (!await reader.Database.SqlQuery<bool>($"""
                    SELECT {firstPid} = ANY(pg_blocking_pids({secondPid})) AS "Value"
                    """).SingleAsync(timeout.Token))
                    await Task.Delay(10, timeout.Token);
                Assert.False(second.IsCompleted);
            }
            // The first HTTP response cannot be published before the transaction commits.
            Assert.False(first.IsCompleted);
            server.Gate.Release.TrySetResult();
            await Saved(await first);
            await Saved(await second);
            await using var context = server.Database.CreateContext();
            var rows = await context.SiteTexts.AsNoTracking().ToListAsync();
            var shared = Assert.Single(rows, row => row.Key == "hero.title");
            Assert.Equal(disjoint ? "First" : "Second", shared.Value);
            Assert.Equal(disjoint ? firstId : secondId, shared.UpdatedBy);
            Assert.Equal(disjoint ? 2 : 1, rows.Count);
        }
        finally
        {
            server.Gate.Release.TrySetResult();
            await first.WaitAsync(TimeSpan.FromSeconds(20));
            if (second is not null) await second.WaitAsync(TimeSpan.FromSeconds(20));
        }
    }
}

public sealed class SafeControllerLog : ILogger<SiteTextsController>
{
    public List<string> Messages { get; } = [];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Assert.Null(exception); // SQL/provider exception details must not reach logs through this boundary.
        Messages.Add(formatter(state, exception));
    }
}

public sealed class HttpWriteGate : DbCommandInterceptor
{
    private string? firstId;
    private string? secondId;
    public TaskCompletionSource<int> Written { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource<int> Started { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Arm(string first, string second)
    {
        firstId = first;
        secondId = second;
        Written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (command.CommandText.StartsWith("INSERT INTO \"SiteTexts\"", StringComparison.Ordinal)
            && Equals(command.Parameters[4].Value, secondId))
            Started.TrySetResult(((NpgsqlConnection)command.Connection!).ProcessID);
        return ValueTask.FromResult(result);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        int result, CancellationToken cancellationToken = default)
    {
        if (command.CommandText.StartsWith("INSERT INTO \"SiteTexts\"", StringComparison.Ordinal)
            && Equals(command.Parameters[4].Value, firstId))
        {
            Written.TrySetResult(((NpgsqlConnection)command.Connection!).ProcessID);
            await Release.Task.WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);
        }
        return result;
    }
}
