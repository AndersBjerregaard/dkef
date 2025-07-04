
using System.Text.Json;

using Minio;
using Minio.DataModel.Args;

namespace Dkef.Services;

public class MinioBucketService(IMinioClient _minioClient) : IBucketService
{
    public async Task<string> GetPresignedUrlAsync(string bucket, string objectName, bool isPublic = false)
    {
        // Make a bucket on the server, if not already present
        var beArgs = new BucketExistsArgs()
            .WithBucket(bucket);
        var found = await _minioClient.BucketExistsAsync(beArgs);


        if (!found)
        {
            var mbArgs = new MakeBucketArgs()
                .WithBucket(bucket);
            await _minioClient.MakeBucketAsync(mbArgs);

            if (isPublic)
            {
                await SetPublicReadPolicy(bucket);
            }
        }
        var psPutArgs = new PresignedPutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithExpiry(3600); // Expiry in seconds
        return await _minioClient.PresignedPutObjectAsync(psPutArgs);
    }


    /// <summary>
    /// Sets the policy for the given bucket to be "public read".
    /// No longer needing presigned URLs for reading existing objects from the bucket.
    /// You can directly construct a public URL for an existing object like: 
    /// http://MinIO_Endpoint/BucketName/ObjectName
    /// </summary>
    private async Task SetPublicReadPolicy(string bucket)
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

        await _minioClient.SetPolicyAsync(setPolicyArgs);
    }
}