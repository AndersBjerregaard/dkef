using Dkef.Domain;
using Dkef.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dkef.Tests;

public class SiteImagePostgresTests(SiteTextPostgresDatabase database) : IClassFixture<SiteTextPostgresDatabase>
{
    private static SiteImage Image(string group, string key) => new()
    {
        Group = group, Slot = "hero", ObjectKey = key, Alt = "Elmast", UpdatedAt = DateTime.UtcNow, UpdatedBy = "identity-id"
    };

    [PostgresFact]
    public async Task RoundTripReplaceAndResetReturnOnlyThePreviousCommittedObject()
    {
        var group = "test-" + Guid.NewGuid().ToString("N");
        await using var context = database.CreateContext();
        var repository = new SiteImageRepository(context);
        Assert.Empty(await repository.GetGroupAsync(group, default));
        Assert.Null(await repository.ReplaceAsync(group, "hero", Image(group, "first"), default));
        Assert.Equal("first", await repository.ReplaceAsync(group, "hero", Image(group, "second"), default));
        Assert.Equal("second", Assert.Single(await repository.GetGroupAsync(group, default)).ObjectKey);
        Assert.Equal("second", await repository.ReplaceAsync(group, "hero", null, default));
        Assert.Null(await repository.ReplaceAsync(group, "hero", null, default));
        Assert.Empty(await repository.GetGroupAsync(group, default));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [PostgresFact]
    public async Task ConcurrentFirstUploadsFormAChainWithoutDeletingTheWinner()
    {
        var group = "test-" + Guid.NewGuid().ToString("N");
        var keys = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid().ToString("N")).ToArray();
        var previous = await Task.WhenAll(keys.Select(async key =>
        {
            await using var context = database.CreateContext();
            return await new SiteImageRepository(context).ReplaceAsync(group, "hero", Image(group, key), default);
        }));
        await using var reader = database.CreateContext();
        var winner = Assert.Single(await new SiteImageRepository(reader).GetGroupAsync(group, default)).ObjectKey;
        Assert.Single(previous, value => value is null);
        Assert.Equal(5, previous.Where(value => value is not null).Distinct().Count());
        Assert.DoesNotContain(winner, previous);
        Assert.All(keys.Where(key => key != winner), key => Assert.Contains(key, previous));
    }
}
