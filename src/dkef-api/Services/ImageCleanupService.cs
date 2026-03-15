using Dkef.Data;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

namespace Dkef.Services;

/// <summary>
/// Service for cleaning up orphaned images in MinIO buckets.
/// An image is considered orphaned if it exists in a MinIO bucket but is not referenced
/// by any Event, News, or GeneralAssembly record in the database.
/// </summary>
public class ImageCleanupService(
    IMinioClient minioClient,
    EventsContext eventsContext,
    NewsContext newsContext,
    GeneralAssemblyContext generalAssemblyContext)
{
    /// <summary>
    /// Scans all image buckets and deletes orphaned images.
    /// Returns statistics about the cleanup operation.
    /// </summary>
    public async Task<ImageCleanupResult> CleanupOrphanImagesAsync(CancellationToken ct = default)
    {
        var result = new ImageCleanupResult();

        // Cleanup events bucket
        var events = await eventsContext.Events
            .AsNoTracking()
            .Where(e => !string.IsNullOrEmpty(e.ThumbnailUrl))
            .ToListAsync(ct);
        
        var eventsResult = await CleanupBucketAsync("events", "Events",
            () => events
                .Select(e => ExtractGuidFromUrl(e.ThumbnailUrl))
                .Where(g => g != Guid.Empty)
                .Select(g => g.ToString())
                .ToList(),
            ct);
        result.BucketResults.Add(eventsResult);

        // Cleanup news bucket
        var news = await newsContext.News
            .AsNoTracking()
            .Where(n => !string.IsNullOrEmpty(n.ThumbnailUrl))
            .ToListAsync(ct);
        
        var newsResult = await CleanupBucketAsync("news", "News",
            () => news
                .Select(n => ExtractGuidFromUrl(n.ThumbnailUrl))
                .Where(g => g != Guid.Empty)
                .Select(g => g.ToString())
                .ToList(),
            ct);
        result.BucketResults.Add(newsResult);

        // Cleanup general-assemblies bucket
        var generalAssemblies = await generalAssemblyContext.GeneralAssemblies
            .AsNoTracking()
            .Where(ga => !string.IsNullOrEmpty(ga.ThumbnailUrl))
            .ToListAsync(ct);
        
        var gaResult = await CleanupBucketAsync("general-assemblies", "General Assemblies",
            () => generalAssemblies
                .Select(ga => ExtractGuidFromUrl(ga.ThumbnailUrl))
                .Where(g => g != Guid.Empty)
                .Select(g => g.ToString())
                .ToList(),
            ct);
        result.BucketResults.Add(gaResult);

        result.TotalOrphanedImages = result.BucketResults.Sum(b => b.OrphanedImageCount);
        result.TotalDeletedImages = result.BucketResults.Sum(b => b.DeletedImageCount);
        result.TotalFailedDeletes = result.BucketResults.Sum(b => b.FailedDeleteCount);

        return result;
    }

    private async Task<BucketCleanupResult> CleanupBucketAsync(
        string bucketName,
        string displayName,
        Func<List<string>> getReferencedGuids,
        CancellationToken ct = default)
    {
        var result = new BucketCleanupResult
        {
            BucketName = bucketName,
            DisplayName = displayName
        };

        try
        {
            Console.WriteLine($"\n--- Scanning {displayName} bucket ---");

            // Get all referenced GUIDs from database
            var referencedGuids = new HashSet<string>(getReferencedGuids(), StringComparer.OrdinalIgnoreCase);
            result.ReferencedImageCount = referencedGuids.Count;
            Console.WriteLine($"Found {referencedGuids.Count} referenced images in database");

            // List all objects in the bucket
            var allObjectNames = new List<string>();
            var listArgs = new ListObjectsArgs().WithBucket(bucketName).WithRecursive(true);

            // MinIO SDK returns an IObservable, so we need to collect items into a list
            var items = new List<Minio.DataModel.Item>();
            var itemsReceived = new TaskCompletionSource<bool>();
            var subscription = minioClient.ListObjectsAsync(listArgs, ct).Subscribe(
                onNext: item => items.Add(item),
                onError: error => itemsReceived.TrySetException(error),
                onCompleted: () => itemsReceived.TrySetResult(true)
            );

            await itemsReceived.Task;
            subscription?.Dispose();

            foreach (var item in items)
            {
                if (!item.IsDir)
                {
                    allObjectNames.Add(item.Key);
                }
            }

            result.TotalImageCount = allObjectNames.Count;
            Console.WriteLine($"Found {allObjectNames.Count} total images in {displayName} bucket");

            // Find orphaned objects
            var orphanedObjects = allObjectNames
                .Where(name => !referencedGuids.Contains(name))
                .ToList();

            result.OrphanedImageCount = orphanedObjects.Count;
            Console.WriteLine($"Found {orphanedObjects.Count} orphaned images");

            // Delete orphaned objects
            foreach (var objectName in orphanedObjects)
            {
                try
                {
                    var rmArgs = new RemoveObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName);
                    await minioClient.RemoveObjectAsync(rmArgs, ct);
                    result.DeletedImageCount++;
                    Console.WriteLine($"  ✓ Deleted: {objectName}");
                }
                catch (Exception ex)
                {
                    result.FailedDeleteCount++;
                    Console.WriteLine($"  ✗ Failed to delete {objectName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error scanning {displayName} bucket: {ex.Message}");
            result.Error = ex.Message;
        }

        return result;
    }

    private static Guid ExtractGuidFromUrl(string url)
    {
        try
        {
            var parts = url.Split('/');
            var lastPart = parts[^1];
            return Guid.TryParse(lastPart, out var guid) ? guid : Guid.Empty;
        }
        catch
        {
            return Guid.Empty;
        }
    }
}

/// <summary>
/// Results of an image cleanup operation.
/// </summary>
public class ImageCleanupResult
{
    public List<BucketCleanupResult> BucketResults { get; set; } = [];
    public int TotalOrphanedImages { get; set; }
    public int TotalDeletedImages { get; set; }
    public int TotalFailedDeletes { get; set; }

    public override string ToString()
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("\n=== Image Cleanup Summary ===");
        summary.AppendLine($"Total orphaned images found: {TotalOrphanedImages}");
        summary.AppendLine($"Total images deleted: {TotalDeletedImages}");
        summary.AppendLine($"Total deletion failures: {TotalFailedDeletes}");
        summary.AppendLine("\nBy bucket:");

        foreach (var bucket in BucketResults)
        {
            summary.AppendLine(bucket.ToString());
        }

        return summary.ToString();
    }
}

/// <summary>
/// Results for a single bucket cleanup operation.
/// </summary>
public class BucketCleanupResult
{
    public string BucketName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int ReferencedImageCount { get; set; }
    public int TotalImageCount { get; set; }
    public int OrphanedImageCount { get; set; }
    public int DeletedImageCount { get; set; }
    public int FailedDeleteCount { get; set; }
    public string? Error { get; set; }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Error))
        {
            return $"  {DisplayName}: ERROR - {Error}";
        }

        return $"  {DisplayName}: {ReferencedImageCount} referenced, {TotalImageCount} total, " +
               $"{OrphanedImageCount} orphaned, {DeletedImageCount} deleted, {FailedDeleteCount} failed";
    }
}
