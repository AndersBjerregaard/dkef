
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Dkef.Services;

public class MinioBucketService(
    IMinioClient _minioClient,
    [FromKeyedServices("MinioInternal")] string _internalEndpoint,
    IConfiguration _config) : IBucketService
{
    // Cache credentials and flags read from config so we don't re-read on every request.
    private readonly string _accessKey = _config.GetSection("Minio")["AccessKey"]!;
    private readonly string _secretKey = _config.GetSection("Minio")["SecretKey"]!;
    private readonly bool _secure = bool.Parse(_config.GetSection("Minio")["Secure"] ?? "false");
    private readonly string? _publicEndpoint = _config.GetSection("Minio")["PublicEndpoint"];

    // Builds a short-lived client pointed at the internal cluster endpoint.
    // Used for admin operations so they bypass the public ingress.
    private IMinioClient BuildInternalClient() =>
        new MinioClient()
            .WithEndpoint(_internalEndpoint)
            .WithCredentials(_accessKey, _secretKey)
            .WithSSL(_secure)
            .Build();

    public async Task<string> GetPresignedUrlAsync(string bucket, string objectName, bool isPublic = false)
    {
        await EnsureBucketAsync(bucket, isPublic);

        // Presigned URL is generated using the public-facing client so the URL
        // it returns contains the public hostname, making it browser-accessible.
        var psPutArgs = new PresignedPutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithExpiry(3600); // Expiry in seconds
        var presignedUrl = await _minioClient.PresignedPutObjectAsync(psPutArgs);

        // TLS may terminate at the public reverse proxy; the signature does not cover the scheme.
        if (!_secure && !string.IsNullOrWhiteSpace(_publicEndpoint))
            presignedUrl = "https" + presignedUrl[4..];

        return presignedUrl;
    }

    public async Task EnsureBucketAsync(string bucket, bool isPublic, CancellationToken ct = default)
    {
        // Admin operations use the internal client so they reach MinIO directly
        // without routing through the public ingress (which would deny HEAD/POST requests).
        using var internalClient = BuildInternalClient();

        var beArgs = new BucketExistsArgs().WithBucket(bucket);
        var found = await internalClient.BucketExistsAsync(beArgs, ct);

        if (!found)
        {
            try
            {
                await internalClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);
            }
            catch (MinioException)
            {
                // Another first upload may have created the bucket after our existence check.
                if (!await internalClient.BucketExistsAsync(beArgs, ct)) throw;
            }
        }
        // Also repairs a prior creation where setting the policy failed.
        if (isPublic) await SetPublicReadPolicy(internalClient, bucket);
    }

    public async Task<bool> DeleteObjectAsync(string bucket, string objectName)
    {
        try
        {
            var internalClient = BuildInternalClient();
            var rmArgs = new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName);
            await internalClient.RemoveObjectAsync(rmArgs);
            return true;
        }
        catch (Exception ex)
        {
            // Don't throw - cleanup failures should not block saves.
            // Log for observability but allow the database change to commit.
            Console.WriteLine($"Warning: Failed to delete {bucket}/{objectName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sets the policy for the given bucket to be "public read".
    /// No longer needing presigned URLs for reading existing objects from the bucket.
    /// You can directly construct a public URL for an existing object like: 
    /// http://MinIO_Endpoint/BucketName/ObjectName
    /// </summary>
    private async Task SetPublicReadPolicy(IMinioClient client, string bucket)
    {
        // The AWS S3 compatible JSON for public read access.
        // There is no struct or constant that the MinIO .NET SDK provides
        // to add a better strongly typed experience for this sadly.
        var policyJson = new
        {
            Version = "2012-10-17",
            Statement = new[]
            {
                new
                {
                    Effect = "Allow",
                    Principal = new { AWS = new[] { "*" } },
                    Action = new[] { "s3:GetObject" },
                    Resource = new[] { $"arn:aws:s3:::{bucket}/*" }
                }
            }
        };

        var policyString = JsonSerializer.Serialize(policyJson);

        var setPolicyArgs = new SetPolicyArgs()
            .WithBucket(bucket)
            .WithPolicy(policyString);

        await client.SetPolicyAsync(setPolicyArgs);
    }
}
