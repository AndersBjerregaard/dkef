using System.Data.Common;

using Dkef.Data;
using Dkef.Domain;
using Dkef.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql;

using Xunit;

namespace Dkef.Tests;

// Opt in explicitly. The fixture creates/drops only its own uniquely named database and never
// starts Program.cs, loads application settings, or migrates any configured application database.
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DKEF_TEST_POSTGRES")))
            Skip = "Set DKEF_TEST_POSTGRES to a disposable local PostgreSQL admin connection.";
    }
}

public sealed class PostgresTheoryAttribute : TheoryAttribute
{
    public PostgresTheoryAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DKEF_TEST_POSTGRES")))
            Skip = "Set DKEF_TEST_POSTGRES to a disposable local PostgreSQL admin connection.";
    }
}

public sealed class SiteTextPostgresDatabase : IAsyncLifetime
{
    private readonly string database = "dkef_site_text_tests_" + Guid.NewGuid().ToString("N");
    private string? adminConnection;
    private string connection = string.Empty;
    private bool created;

    public ContentsContext CreateContext(params IInterceptor[] interceptors) => new(
        new DbContextOptionsBuilder<ContentsContext>().UseNpgsql(connection).AddInterceptors(interceptors).Options);

    public ContactsContext CreateContactsContext() => new(
        new DbContextOptionsBuilder<ContactsContext>().UseNpgsql(connection).Options);

    public async Task InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable("DKEF_TEST_POSTGRES");
        if (string.IsNullOrEmpty(configured)) return;
        var builder = new NpgsqlConnectionStringBuilder(configured);
        if (builder.Host is not ("localhost" or "127.0.0.1" or "::1"))
            throw new InvalidOperationException("Persistence tests require a loopback PostgreSQL host.");
        builder.Database = "postgres";
        builder.Pooling = false;
        builder.Timeout = 10;
        builder.CommandTimeout = 15;
        adminConnection = builder.ConnectionString;
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        // The identifier is generated above, never taken from configuration or request input.
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin);
        await create.ExecuteNonQueryAsync();
        created = true;
        builder.Database = database;
        connection = builder.ConnectionString;
        try
        {
            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (!created) return;
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
        created = false;
    }
}

public class SiteTextPostgresTests(SiteTextPostgresDatabase database) : IClassFixture<SiteTextPostgresDatabase>
{
    private static string NewGroup() => "test-" + Guid.NewGuid().ToString("N");

    [PostgresFact]
    public async Task RoundTripsValuesAndUpdatesOnlySubmittedKeysWithOneServerTimestamp()
    {
        await using var context = database.CreateContext();
        var repository = new SiteTextRepository(context);
        var group = NewGroup();
        Assert.Empty(await repository.GetGroupAsync(group));
        var updater = Guid.NewGuid().ToString();
        var before = DateTime.UtcNow;
        var initial = await repository.SaveBatchAsync(group, new Dictionary<string, string>
        {
            ["title"] = "Ændret øvelse & 2 < 3\nNy linje",
            ["optional"] = "",
            ["default"] = SiteTextRegistryTests.Registry.GetFields("home")["hero.title"].Default
        }, updater);
        Assert.Equal(3, initial.Count);
        Assert.All(initial, text =>
        {
            Assert.Equal(group, text.Group);
            Assert.Equal(updater, text.UpdatedBy);
            Assert.Equal(DateTimeKind.Utc, text.UpdatedAt.Kind);
            Assert.InRange(text.UpdatedAt, before.AddMilliseconds(-1), DateTime.UtcNow);
        });
        Assert.Single(initial.Select(text => text.UpdatedAt).Distinct());
        Assert.Empty(context.ChangeTracker.Entries<SiteText>());
        await repository.SaveBatchAsync(group + "-other", new Dictionary<string, string> { ["title"] = "Anden gruppe" }, updater);

        // A tracked old value must not make the raw upsert's response stale.
        await context.SiteTexts.SingleAsync(text => text.Group == group && text.Key == "title");
        var nextUpdater = Guid.NewGuid().ToString();
        var saved = await repository.SaveBatchAsync(group, new Dictionary<string, string> { ["title"] = "Nyt" }, nextUpdater);
        var title = Assert.Single(saved, text => text.Key == "title");
        Assert.Equal("Nyt", title.Value);
        Assert.Equal(nextUpdater, title.UpdatedBy);
        Assert.Equal("", Assert.Single(saved, text => text.Key == "optional").Value);
        foreach (var omitted in initial.Where(text => text.Key != "title"))
        {
            var unchanged = Assert.Single(saved, text => text.Key == omitted.Key);
            Assert.Equal(omitted.Value, unchanged.Value);
            Assert.Equal(omitted.UpdatedAt, unchanged.UpdatedAt);
            Assert.Equal(omitted.UpdatedBy, unchanged.UpdatedBy);
        }
        await using var reader = database.CreateContext();
        var committed = await new SiteTextRepository(reader).GetGroupAsync(group);
        Assert.Equal("Nyt", Assert.Single(committed, text => text.Key == "title").Value);
        Assert.Equal("Anden gruppe", Assert.Single(await repository.GetGroupAsync(group + "-other")).Value);
    }

    [PostgresFact]
    public async Task CompositeKeyAndIdentifierBoundsAreEnforcedByPostgres()
    {
        var group = NewGroup();
        await using (var context = database.CreateContext())
            await new SiteTextRepository(context).SaveBatchAsync(group, new Dictionary<string, string> { ["title"] = "Original" }, "identity-id");
        await using (var context = database.CreateContext())
        {
            context.SiteTexts.Add(new SiteText { Group = group, Key = "title", Value = "Duplicate", UpdatedAt = DateTime.UtcNow, UpdatedBy = "identity-id" });
            var error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
        }
        await using (var context = database.CreateContext())
        {
            var repository = new SiteTextRepository(context);
            var bound = new string('a', 128);
            Assert.Single(await repository.SaveBatchAsync(bound, new Dictionary<string, string> { [bound] = "At bound" }, "identity-id"));
            foreach (var (badGroup, badKey) in new[] { (bound + "a", "key"), (group, bound + "a") })
            {
                var error = await Assert.ThrowsAsync<PostgresException>(() => repository.SaveBatchAsync(
                    badGroup, new Dictionary<string, string> { [badKey] = "Too long" }, "identity-id"));
                Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, error.SqlState);
            }
        }
    }

    [PostgresFact]
    public async Task DatabaseFailureRollsBackEarlierInsertsAndUpdatesInBatch()
    {
        await using var context = database.CreateContext();
        var repository = new SiteTextRepository(context);
        var group = NewGroup();
        var original = Assert.Single(await repository.SaveBatchAsync(group, new Dictionary<string, string> { ["a.existing"] = "Original" }, "original-id"));
        await context.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "SiteTexts" ADD CONSTRAINT "TestRejectedValue" CHECK ("Value" <> 'database-rejected')
            """);
        try
        {
            var error = await Assert.ThrowsAsync<PostgresException>(() => repository.SaveBatchAsync(group,
                new Dictionary<string, string>
                {
                    ["z.failure"] = "database-rejected",
                    ["b.insert"] = "Must roll back",
                    ["a.existing"] = "Must also roll back"
                }, "next-id"));
            Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
            await using var reader = database.CreateContext();
            var remaining = Assert.Single(await new SiteTextRepository(reader).GetGroupAsync(group));
            Assert.Equal(original.Value, remaining.Value);
            Assert.Equal(original.UpdatedAt, remaining.UpdatedAt);
            Assert.Equal(original.UpdatedBy, remaining.UpdatedBy);
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"SiteTexts\" DROP CONSTRAINT \"TestRejectedValue\"");
        }
    }

    [PostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResponseReadOrCommitFailureDoesNotPublishBatch(bool failCommit)
    {
        var group = NewGroup();
        await using var context = database.CreateContext(
            failCommit ? new FailCommit() : new FailResponseRead());
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SiteTextRepository(context).SaveBatchAsync(
            group, new Dictionary<string, string> { ["title"] = "Must not publish" }, "identity-id"));
        await using var reader = database.CreateContext();
        Assert.Empty(await new SiteTextRepository(reader).GetGroupAsync(group));
    }

    [PostgresFact]
    public async Task SqlLikeInputRemainsParameterizedData()
    {
        await using var context = database.CreateContext();
        var repository = new SiteTextRepository(context);
        var group = NewGroup() + "'; DROP TABLE \"SiteTexts\"; --";
        const string key = "title'); DELETE FROM \"SiteTexts\"; --";
        const string value = "Æble ' \\ & 2 < 3; UPDATE \"SiteTexts\" SET \"Value\" = 'bad'";
        const string updater = "identity-id'; --";
        var saved = Assert.Single(await repository.SaveBatchAsync(group,
            new Dictionary<string, string> { [key] = value }, updater));
        Assert.Equal(group, saved.Group);
        Assert.Equal(key, saved.Key);
        Assert.Equal(value, saved.Value);
        Assert.Equal(updater, saved.UpdatedBy);
        Assert.Equal(value, Assert.Single(await repository.GetGroupAsync(group)).Value);
    }

    [PostgresTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ConcurrentSavesPreserveDisjointKeysAndLastCommitWinsForOverlaps(bool existing, bool disjoint)
    {
        var group = NewGroup();
        await using var reader = database.CreateContext();
        if (existing)
            await new SiteTextRepository(reader).SaveBatchAsync(group, new Dictionary<string, string> { ["a.shared"] = "Before" }, "before-id");

        var gate = new UpsertGate();
        var secondStarted = new UpsertStarted();
        await using var firstContext = database.CreateContext(gate);
        await using var secondContext = database.CreateContext(secondStarted);
        var first = new SiteTextRepository(firstContext).SaveBatchAsync(group,
            new Dictionary<string, string> { ["z.first"] = "Only first", ["a.shared"] = "First" }, "first-id");
        Task<IReadOnlyList<SiteText>>? second = null;
        try
        {
            var firstPid = await gate.Written.Task.WaitAsync(TimeSpan.FromSeconds(10));
            // First transaction has written but not committed; outside readers cannot see it.
            Assert.Equal(existing ? "Before" : null,
                (await new SiteTextRepository(reader).GetGroupAsync(group)).SingleOrDefault(text => text.Key == "a.shared")?.Value);
            second = new SiteTextRepository(secondContext).SaveBatchAsync(group,
                new Dictionary<string, string>
                {
                    [disjoint ? "b.disjoint" : "a.shared"] = "Second",
                    ["y.second"] = "Only second"
                }, "second-id");
            var secondPid = await secondStarted.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            if (disjoint)
                await second.WaitAsync(TimeSpan.FromSeconds(10)); // No group-wide lock.
            else
            {
                // Observe actual PostgreSQL lock contention, not just two tasks scheduled together.
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                while (!await reader.Database.SqlQuery<bool>($"""
                    SELECT {firstPid} = ANY(pg_blocking_pids({secondPid})) AS "Value"
                    """).SingleAsync(timeout.Token))
                    await Task.Delay(10, timeout.Token);
                Assert.False(second.IsCompleted); // Overlapping upsert must wait for the first row lock.
            }
            gate.Release.TrySetResult();
            var firstResult = await first.WaitAsync(TimeSpan.FromSeconds(10));
            var secondResult = await second.WaitAsync(TimeSpan.FromSeconds(10));
            var lastResult = disjoint ? firstResult : secondResult;
            Assert.Equal(disjoint ? 4 : 3, lastResult.Count);
            Assert.Equal(disjoint ? "First" : "Second", Assert.Single(lastResult, text => text.Key == "a.shared").Value);
            Assert.Equal(["a.shared", "z.first"], gate.Keys);

            // A completed save has committed, and the final updater belongs to the winning batch.
            var final = await new SiteTextRepository(reader).GetGroupAsync(group);
            Assert.Equal(lastResult.Count, final.Count);
            Assert.Equal(disjoint ? "first-id" : "second-id", Assert.Single(final, text => text.Key == "a.shared").UpdatedBy);
            Assert.Equal("Only first", Assert.Single(final, text => text.Key == "z.first").Value);
            Assert.Equal("Only second", Assert.Single(final, text => text.Key == "y.second").Value);
        }
        finally
        {
            gate.Release.TrySetResult();
            await first.WaitAsync(TimeSpan.FromSeconds(20));
            if (second is not null) await second.WaitAsync(TimeSpan.FromSeconds(20));
        }
    }

    [PostgresFact]
    public async Task DeletingContactRetainsPublishedTextAndIdentityAudit()
    {
        await using var contacts = database.CreateContactsContext();
        await contacts.Database.MigrateAsync();
        var contact = new Contact { Id = Guid.NewGuid().ToString() };
        contacts.Contacts.Add(contact);
        await contacts.SaveChangesAsync();
        await using var context = database.CreateContext();
        var repository = new SiteTextRepository(context);
        var group = NewGroup();
        await repository.SaveBatchAsync(group, new Dictionary<string, string> { ["title"] = "Published" }, contact.Id);
        contacts.Contacts.Remove(contact);
        await contacts.SaveChangesAsync();
        var retained = Assert.Single(await repository.GetGroupAsync(group));
        Assert.Equal("Published", retained.Value);
        Assert.Equal(contact.Id, retained.UpdatedBy);
    }

    [PostgresFact]
    public async Task MigrationCanRollBackAndReapplyWithoutChangingExistingContents()
    {
        await using var context = database.CreateContext();
        var content = new News { Title = "Existing content", Description = "Unchanged", DateTime = DateTime.UtcNow };
        context.News.Add(content);
        await context.SaveChangesAsync();
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20260911172703_AddAttachmentFileNames");
        // Up/Down must touch only SiteTexts. Contents and its discriminator stay intact.
        Assert.Equal("Existing content", (await context.News.AsNoTracking().SingleAsync(item => item.Id == content.Id)).Title);
        await using (var absent = database.CreateContext())
        {
            var error = await Assert.ThrowsAsync<PostgresException>(() => absent.SiteTexts.CountAsync());
            Assert.Equal(PostgresErrorCodes.UndefinedTable, error.SqlState);
        }
        await migrator.MigrateAsync();
        Assert.Empty(await context.SiteTexts.AsNoTracking().ToListAsync()); // No default rows seeded.
        Assert.Equal("Unchanged", (await context.News.AsNoTracking().SingleAsync(item => item.Id == content.Id)).Description);
    }

    private sealed class UpsertGate : DbCommandInterceptor
    {
        public TaskCompletionSource<int> Written { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public List<string> Keys { get; } = [];

        public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
            int result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("INSERT INTO \"SiteTexts\"", StringComparison.Ordinal))
            {
                Keys.Add((string)command.Parameters[1].Value!);
                if (Keys.Count == 1)
                {
                    Written.TrySetResult(((NpgsqlConnection)command.Connection!).ProcessID);
                    await Release.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
                }
            }
            return result;
        }
    }

    private sealed class FailResponseRead : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Injected response-read failure.");
    }

    private sealed class FailCommit : DbTransactionInterceptor
    {
        public override ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction,
            TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Injected pre-commit failure.");
    }

    private sealed class UpsertStarted : DbCommandInterceptor
    {
        public TaskCompletionSource<int> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("INSERT INTO \"SiteTexts\"", StringComparison.Ordinal))
                Started.TrySetResult(((NpgsqlConnection)command.Connection!).ProcessID);
            return ValueTask.FromResult(result);
        }
    }
}
