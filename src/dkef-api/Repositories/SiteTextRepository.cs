using System.Data;

using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public sealed class SiteTextRepository(ContentsContext context)
{
    public async Task<IReadOnlyList<SiteText>> GetGroupAsync(string group, CancellationToken cancellationToken = default)
        => await context.SiteTexts.AsNoTracking()
            .Where(text => text.Group == group)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Publishes already validated changes. The caller must supply the authenticated Identity ID
    /// from SiteTextAuditIdentity, never an ID from request data. Field validation/sanitization and
    /// filtering retired keys from public responses belong to the API boundary.
    /// </summary>
    public async Task<IReadOnlyList<SiteText>> SaveBatchAsync(
        string group,
        IReadOnlyDictionary<string, string> changes,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);
        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Count == 0) throw new ArgumentException("A site-text batch must contain changes.", nameof(changes));

        // Snapshot and sort before taking locks. Disjoint saves never replace a stale whole group.
        var writes = changes.OrderBy(change => change.Key, StringComparer.Ordinal).ToArray();
        var updatedAt = DateTime.UtcNow;
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        foreach (var (key, value) in writes)
        {
            // EF parameterizes every interpolated value. ON CONFLICT also handles concurrent first
            // inserts; PostgreSQL holds row locks until commit (last-committed-write-wins per key).
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "SiteTexts" ("Group", "Key", "Value", "UpdatedAt", "UpdatedBy")
                VALUES ({group}, {key}, {value}, {updatedAt}, {updatedBy})
                ON CONFLICT ("Group", "Key") DO UPDATE SET
                    "Value" = EXCLUDED."Value",
                    "UpdatedAt" = EXCLUDED."UpdatedAt",
                    "UpdatedBy" = EXCLUDED."UpdatedBy"
                """, cancellationToken);
        }

        // Untracked so raw upserts cannot leave the response using stale tracked entities.
        var saved = await GetGroupAsync(group, cancellationToken);
        // Disposal rolls back if any write, response read, or commit fails; never return partial data.
        await transaction.CommitAsync(cancellationToken);
        return saved;
    }
}
