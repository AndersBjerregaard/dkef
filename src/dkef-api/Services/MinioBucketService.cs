
using Minio;
using Minio.DataModel.Args;

namespace Dkef.Services;

public class MinioBucketService(IMinioClient _minioClient) : IBucketService
{
    public async Task<string> GetPresignedUrlAsync(string bucket, string objectName)
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
        }
        var psPutArgs = new PresignedPutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithExpiry(3600); // Expiry in seconds
        return await _minioClient.PresignedPutObjectAsync(psPutArgs);
    }
}