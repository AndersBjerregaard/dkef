using Dkef.Data;
using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public interface ISiteImageRepository
{
    Task<IReadOnlyList<SiteImage>> GetGroupAsync(string group, CancellationToken ct);
    Task<string?> ReplaceAsync(string group, string slot, SiteImage? replacement, CancellationToken ct);
}

public sealed class SiteImageRepository(ContentsContext context) : ISiteImageRepository
{
    public async Task<IReadOnlyList<SiteImage>> GetGroupAsync(string group, CancellationToken ct) =>
        await context.SiteImages.AsNoTracking().Where(image => image.Group == group).ToListAsync(ct);

    // Serialize writes per slot, including concurrent first uploads/resets. Every upload has a
    // unique key; returning the previous committed key makes post-commit deletion race-safe.
    public async Task<string?> ReplaceAsync(string group, string slot, SiteImage? replacement, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        var lockKey = $"site-images/{group}/{slot}";
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", ct);
        var previous = await context.SiteImages.SingleOrDefaultAsync(image => image.Group == group && image.Slot == slot, ct);
        var previousKey = previous?.ObjectKey;
        if (replacement is null)
        {
            if (previous is not null) context.SiteImages.Remove(previous);
        }
        else if (previous is null)
        {
            context.SiteImages.Add(replacement);
        }
        else
        {
            context.Entry(previous).CurrentValues.SetValues(replacement);
        }
        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return previousKey;
    }
}
