using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Dkef.Contracts;
using Dkef.Controllers;
using Dkef.Data;
using Dkef.Repositories;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace Dkef.Tests;

public class SiteTextControllerTests
{
    private static SiteTextsController Controller(Stream body, bool identity = true)
    {
        // No connection is opened by these rejection tests; any attempted persistence fails the test.
        var context = new ContentsContext(new DbContextOptionsBuilder<ContentsContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=unused;Timeout=1").Options);
        var claim = new Claim(ClaimTypes.NameIdentifier, "identity-id");
        claim.Properties[JwtSecurityTokenHandler.ShortClaimTypeProperty] = "nameid";
        return new(new SiteTextRepository(context), SiteTextRegistryTests.Registry, NullLogger<SiteTextsController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(identity ? [claim] : [], "test")),
                    Request = { Body = body }
                }
            }
        };
    }

    [Fact]
    public async Task UnknownGroupsAndMissingAuditIdentityNeverReadTheBody()
    {
        using var body = new CountingBody([], forbidReads: true);
        var controller = Controller(body);
        Assert.IsType<NotFoundResult>(await controller.GetGroup("Home", default));
        Assert.Equal("no-store", controller.Response.Headers.CacheControl);
        Assert.IsType<NotFoundResult>(await controller.PatchGroup("Home", default));
        Assert.IsType<ForbidResult>(await Controller(body, identity: false).PatchGroup("home", default));
        Assert.Equal(0, body.BytesRead);
    }

    [Fact]
    public async Task ContentLengthIsRejectedBeforeReading()
    {
        using var body = new CountingBody([], forbidReads: true);
        var controller = Controller(body);
        controller.Request.ContentLength = SiteTextPatchContract.MaxRequestBytes(SiteTextRegistryTests.Registry, "home") + 1;
        var result = Assert.IsType<ObjectResult>(await controller.PatchGroup("home", default));
        Assert.Equal(413, result.StatusCode);
        Assert.Contains("request", Assert.IsType<ValidationProblemDetails>(result.Value).Errors.Keys);
        Assert.Equal(0, body.BytesRead);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1L)]
    public async Task NonSeekableBodiesCannotBypassTheBoundWithMissingOrFalseLength(long? contentLength)
    {
        var maximum = (int)SiteTextPatchContract.MaxRequestBytes(SiteTextRegistryTests.Registry, "home");
        using var body = new CountingBody(Encoding.UTF8.GetBytes(new string(' ', maximum + 10000)));
        var controller = Controller(body);
        controller.Request.ContentLength = contentLength;
        var result = Assert.IsType<ObjectResult>(await controller.PatchGroup("home", default));
        Assert.Equal(413, result.StatusCode);
        Assert.Equal(maximum + 1, body.BytesRead);
    }

    [Theory]
    [InlineData("{}", "request")]
    [InlineData("{", "request")]
    [InlineData("{\"changes\":{\"hero.title\":\"A\",\"hero.\\u0074itle\":\"B\"}}", "request")]
    [InlineData("{\"changes\":{\"hero.title\":\"A\",\"hero.description\":\" \"}}", "changes.hero.description")]
    [InlineData("{\"changes\":{\"hero.title\":\"<b></b>\"}}", "changes.hero.title")]
    public async Task ParserErrorsAreReturnedWithoutPersistence(string json, string path)
    {
        using var body = new CountingBody(Encoding.UTF8.GetBytes(json));
        var result = Assert.IsType<BadRequestObjectResult>(await Controller(body).PatchGroup("home", default));
        var problem = Assert.IsType<ValidationProblemDetails>(result.Value);
        Assert.Equal(400, problem.Status);
        Assert.Equal("En eller flere tekster er ugyldige.", problem.Title);
        Assert.Contains(path, problem.Errors.Keys);
    }

    [Fact]
    public async Task RequestCancellationIsNotConvertedToAServerError()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var body = new CountingBody(Encoding.UTF8.GetBytes("{}"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Controller(body).PatchGroup("home", cancellation.Token));
    }

    [Theory]
    [InlineData(400)]
    [InlineData(413)]
    public async Task ServerRejectedRequestStreamsReturnSafeRequestProblems(int status)
    {
        using var body = new CountingBody([], errorStatus: status);
        var result = Assert.IsType<ObjectResult>(await Controller(body).PatchGroup("home", default));
        Assert.Equal(status, result.StatusCode);
        var problem = Assert.IsType<ValidationProblemDetails>(result.Value);
        Assert.Contains("request", problem.Errors.Keys);
        Assert.DoesNotContain("private-stream-detail", problem.Errors["request"][0]);
    }

    // Simulates request streams with no seeking/Length and small, irregular chunks.
    private sealed class CountingBody(byte[] bytes, bool forbidReads = false, int? errorStatus = null) : Stream
    {
        public int BytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (forbidReads) throw new InvalidOperationException("Body must not be read.");
            if (errorStatus is { } status) throw new BadHttpRequestException("private-stream-detail", status);
            cancellationToken.ThrowIfCancellationRequested();
            var count = Math.Min(Math.Min(buffer.Length, 317), bytes.Length - BytesRead);
            bytes.AsMemory(BytesRead, count).CopyTo(buffer);
            BytesRead += count;
            return ValueTask.FromResult(count);
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
