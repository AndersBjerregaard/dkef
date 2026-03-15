namespace Dkef.Services;

public interface IBucketService
{
    Task<string> GetPresignedUrlAsync(string bucket, string objectName, bool isPublic = false);
    Task<bool> DeleteObjectAsync(string bucket, string objectName);
}