using System.Net;
using Dkef.Services;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Xunit;

namespace Dkef.Tests;

public sealed class MinioFactAttribute : FactAttribute
{
    public MinioFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DKEF_TEST_MINIO")))
            Skip = "Set DKEF_TEST_MINIO to a fresh disposable loopback MinIO endpoint (host:port).";
    }
}

public class SiteImageMinioTests
{
    [MinioFact]
    public async Task InternalUploadCreatesPublicBucketAndDeletesThroughInternalEndpoint()
    {
        var endpoint = Environment.GetEnvironmentVariable("DKEF_TEST_MINIO")!;
        if (!Uri.TryCreate("http://" + endpoint, UriKind.Absolute, out var address) ||
            address.Host is not ("localhost" or "127.0.0.1" or "::1"))
            throw new InvalidOperationException("Storage tests require a fresh disposable loopback MinIO endpoint.");
        var access = Environment.GetEnvironmentVariable("DKEF_TEST_MINIO_ACCESS_KEY")
            ?? throw new InvalidOperationException("Set DKEF_TEST_MINIO_ACCESS_KEY.");
        var secret = Environment.GetEnvironmentVariable("DKEF_TEST_MINIO_SECRET_KEY")
            ?? throw new InvalidOperationException("Set DKEF_TEST_MINIO_SECRET_KEY.");
        using var internalClient = new MinioClient().WithEndpoint(endpoint).WithCredentials(access, secret).Build();
        const string bucket = "site-images";
        if (await internalClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket)))
            throw new InvalidOperationException("The disposable MinIO test server must not already have a site-images bucket.");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Minio"] = "127.0.0.1:1", // Public/signing client deliberately cannot reach storage.
            ["Minio:PublicEndpoint"] = "images.example.dk",
            ["Minio:AccessKey"] = access, ["Minio:SecretKey"] = secret, ["Minio:Secure"] = "false"
        }).Build();
        using var publicClient = new MinioClient().WithEndpoint("127.0.0.1:1").WithCredentials(access, secret).Build();
        var buckets = new MinioBucketService(publicClient, endpoint, config);
        using var storage = new SiteImageStorage(endpoint, config, buckets);
        var key = $"home/hero/{Guid.NewGuid():N}.webp";
        try
        {
            using var input = new MemoryStream(SiteImageValidatorTests.ImageBytes());
            using var encoded = await SiteImageValidator.ValidateAndEncodeAsync(input, default);
            var expected = encoded.ToArray();
            await storage.UploadAsync(key, encoded, default);
            Assert.Equal($"https://images.example.dk/{bucket}/{key}", storage.PublicUrl(key));
            using var http = new HttpClient();
            var localUrl = $"http://{endpoint}/{bucket}/{key}";
            using var read = await http.GetAsync(localUrl);
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);
            Assert.Equal("image/webp", read.Content.Headers.ContentType!.MediaType);
            Assert.Equal(expected, await read.Content.ReadAsByteArrayAsync());
            // Existing buckets also retain/repair public-read policy.
            await buckets.EnsureBucketAsync(bucket, true);
            await storage.DeleteAsync(key);
            using var deleted = await http.GetAsync(localUrl);
            Assert.Equal(HttpStatusCode.NotFound, deleted.StatusCode);
        }
        finally
        {
            // Only our uniquely named object and the bucket proven absent above are removed.
            await storage.DeleteAsync(key);
            await internalClient.RemoveBucketAsync(new RemoveBucketArgs().WithBucket(bucket));
        }
    }
}
