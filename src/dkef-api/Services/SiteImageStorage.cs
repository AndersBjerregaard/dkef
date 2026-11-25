using Microsoft.Extensions.DependencyInjection;
using Minio;
using Minio.DataModel.Args;

namespace Dkef.Services;

public interface ISiteImageStorage
{
    string PublicUrl(string objectKey);
    Task UploadAsync(string objectKey, Stream content, CancellationToken ct);
    Task DeleteAsync(string objectKey);
}

public sealed class SiteImageStorage : ISiteImageStorage, IDisposable
{
    private const string Bucket = "site-images";
    private readonly IMinioClient client;
    private readonly IBucketService buckets;
    private readonly string publicBase;

    public SiteImageStorage([FromKeyedServices("MinioInternal")] string internalEndpoint,
        IConfiguration config, IBucketService buckets)
    {
        this.buckets = buckets;
        var secure = config.GetValue<bool>("Minio:Secure");
        client = new MinioClient().WithEndpoint(internalEndpoint)
            .WithCredentials(config["Minio:AccessKey"], config["Minio:SecretKey"])
            .WithSSL(secure).Build();
        var publicEndpoint = config["Minio:PublicEndpoint"];
        publicBase = string.IsNullOrWhiteSpace(publicEndpoint)
            ? $"{(secure ? "https" : "http")}://{config.GetConnectionString("Minio")}"
            : $"https://{publicEndpoint}";
    }

    public string PublicUrl(string objectKey) => $"{publicBase}/{Bucket}/{objectKey}";

    public async Task UploadAsync(string objectKey, Stream content, CancellationToken ct)
    {
        await buckets.EnsureBucketAsync(Bucket, isPublic: true, ct);
        await client.PutObjectAsync(new PutObjectArgs().WithBucket(Bucket).WithObject(objectKey)
            .WithStreamData(content).WithObjectSize(content.Length).WithContentType("image/webp"), ct);
    }

    public async Task DeleteAsync(string objectKey) =>
        await client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(Bucket).WithObject(objectKey));

    public void Dispose() => client.Dispose();
}
