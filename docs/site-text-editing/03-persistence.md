# 03 — Add site-text override persistence

**Status:** Implemented and human approved (2026-11-24)

**Depends on:** [02](02-definitions-and-contract.md)

**Blocks:** [04](04-api.md)

## Goal

Store overrides in the existing database, independently of event/news/general-assembly inheritance, with atomic batch writes and trustworthy audit metadata.

## Files and tasks

1. Add `src/dkef-api/Domain/SiteText.cs` as a standalone entity with `Group`, `Key`, `Value`, `UpdatedAt`, and `UpdatedBy`. Use the `(Group, Key)` composite primary key to enforce uniqueness; use bounded identifier columns aligned with the registry, a text value column, UTC timestamp, and Identity string ID for the updater.
2. Add `DbSet<SiteText>` and explicit table/column mapping to `src/dkef-api/Data/ContentsContext.cs`. Do not derive from `BaseContent`, add a discriminator value, or change existing feed queries. Retain the updater ID as audit data without introducing cross-context cascade deletion of published text when a contact is removed.
3. Generate a migration from `src/dkef-api` using `dotnet ef migrations add AddSiteTexts --context ContentsContext --output-dir Migrations/Contents`. Review the generated migration and snapshot: they should add only the new table/constraints and corresponding model metadata. No default rows should be seeded.
4. Add `src/dkef-api/Repositories/SiteTextRepository.cs` following the repository's scoped patterns, and register it in `Program.cs`. Read all overrides for a group in one query; use untracked reads where appropriate.
5. Implement an explicit transactional batch-save operation for already validated values. Use parameterized PostgreSQL upserts (or an equally small provider-safe equivalent) so concurrent insertion of the same group/key honors last-committed-write-wins instead of producing a unique-key failure. Sort writes by key to reduce inconsistent lock order; do not build SQL from request strings.
6. Update only submitted keys and their audit metadata. Assign one UTC timestamp and server-derived updater ID per batch. Read the current group overrides in the transaction for the response, commit, and return only on success; rollback all submitted changes on any failure.
7. Keep publication/reset semantics explicit: a missing row means default, `""` is a stored optional override, and matching the default does not remove a row. No reset endpoint or broad cleanup of retired overrides is included.

## Acceptance criteria

- [x] The migration creates one separate `SiteTexts` table and no changes to `Contents` inheritance.
- [x] Duplicate group/key rows are impossible; reads and writes are group-scoped.
- [x] Multiple valid changes commit together; a database failure rolls the batch back.
- [x] Concurrent first inserts and overlapping updates follow the documented policy; omitted keys survive.
- [x] Audit metadata is separate from submitted values and is not lost on user deletion. The repository generates timestamps and accepts the trusted server-side Identity ID; 04 must enforce authenticated identity at the HTTP boundary.
- [x] Defaults remain code-owned; a new field/group needs no schema change.

## Validation and handoff

Run `dotnet restore` and `dotnet build`. Review migration SQL without applying it to shared resources. Before database tests or API startup, confirm all configured contexts, including optional `appsettings.Local.json`, point to disposable local resources. Verify apply/rollback on that database, uniqueness, optional empty text, audit timestamps/IDs, batch rollback, and concurrent upsert behavior. Use PostgreSQL, not an in-memory provider, for transaction/constraint checks.

Hand the migration and repository API to 04. Record migration/rollback requirements for the later human-operated rollout; do not apply anything to production.

## Human approval record

- Approved by the requesting human on **2026-11-24**.
- Reviewed implementation: commit `57f2091` (`feat: add transactional site-text persistence`).
- Approval covers item 03's implementation, PostgreSQL validation, and documented
  repository and migration/rollback handoff, following clarification of production
  startup migrations and the repository's transactional upsert design.
- This approval does not authorize deployment or mark step 04's HTTP endpoints,
  authorization, error mapping, or public response filtering complete.

## Implementation and handoff to 04

- `SiteText` is a standalone entity in `ContentsContext`, not a `BaseContent`
  subtype. Migration `20261124223426_AddSiteTexts` creates only `SiteTexts` with
  composite primary key `(Group, Key)`, required `Value`/`UpdatedBy` text columns,
  and required `UpdatedAt` PostgreSQL `timestamp with time zone`.
- Group/key columns allow 128 characters each. Both package-local registry
  loaders now reject identifiers longer than 128 UTF-16 units (also applied to
  section IDs for a consistent identifier rule). Registered identifiers are ASCII,
  so these bounds match PostgreSQL character lengths. Boundary/rejection tests
  cover both loaders and the database. No approved identifier/default changed.
- `SiteTextRepository` is scoped in `Program.cs`. `GetGroupAsync(group, token)`
  reads a group's overrides in one untracked query. It returns stored entities,
  including audit data and any retired keys; **04 must filter registered keys and
  map to `SiteTextGroupResponse`**, not serialize these entities publicly.
- `SaveBatchAsync(group, changes, updatedBy, token)` accepts already validated,
  sanitized changes. In 04, check the registered group and parse the whole request
  with `SiteTextPatchContract` before invoking it once; obtain `updatedBy` only
  from `SiteTextAuditIdentity.FindIdentityId(User)` and fail closed if absent.
  Do not bind audit metadata from request JSON. Persistence does not duplicate
  registry/value validation or authorization, and it rejects blank updater IDs
  and empty batches as programming errors.
- Save snapshots/sorts submitted keys ordinally, generates one `DateTime.UtcNow`
  and uses one supplied Identity ID for the batch, then performs parameterized
  PostgreSQL `INSERT ... ON CONFLICT ... DO UPDATE` statements inside an explicit
  transaction. Only submitted rows and their audit metadata change. PostgreSQL's
  explicit read-committed isolation and row locks permit concurrent first inserts
  and overlapping updates with last-committed-write-wins; disjoint keys are not
  serialized by a group-wide lock.
- The full stored group is read untracked **inside** that transaction, committed,
  and only then returned. Write, response-read, or pre-commit failures dispose/
  roll back the transaction and return no success. The response is a transactional
  snapshot, not a promise to include another transaction's later commit.
- Missing rows, stored `""`, and saved default-equivalent values remain distinct.
  No seed data, reset/delete operation, cleanup of retired keys, contact foreign
  key, or contact-deletion cascade was added. Updater IDs remain historical text
  even after their contact no longer exists.

## Repeatable PostgreSQL validation

`tests/dkef-api.Tests/SiteTextPostgresTests.cs` is opt-in. Ordinary `dotnet test`
continues to run registry/contract/model tests without database access, reporting
the PostgreSQL tests as skipped. To include integration tests:

1. Confirm the Docker context and PostgreSQL instance are local/disposable.
2. Set `DKEF_TEST_POSTGRES` securely to a connection string for that local instance,
   e.g. `Host=127.0.0.1;Port=5432;Database=postgres;Username=<local-role>;Password=<local-password>`.
   The role needs permission to create/drop its own database. Do not commit the
   connection string. With a Windows-hosted `dotnet.exe` invoked from WSL, also
   add `DKEF_TEST_POSTGRES` to `WSLENV` so the test process receives it.
3. Run from the repository root:

   ```sh
   dotnet test tests/dkef-api.Tests/dkef-api.Tests.csproj
   ```

The fixture rejects non-loopback hosts, connects to the `postgres` maintenance
database, and creates a uniquely named `dkef_site_text_tests_<guid>` database.
It migrates only its own database directly through `ContentsContext` (and
`ContactsContext` for the deletion scenario), never starts the API or reads
application connection settings, and drops only that generated database on
teardown. Tests are sequential within the fixture. The concurrency tests hold a
transaction open and observe real PostgreSQL blocking between overlapping
upserts; the disjoint save must complete while the first transaction is open.

### Validation record (2026-11-24)

Validated on the clean, existing `feat/text-modification` task branch after
fetching and verifying its base against GitHub's default branch (`master`).
All six API contexts use the same `PostgresDb` configuration; Development points
to local `localhost:5432`, and `appsettings.Local.json` has no connection overrides.
No connection/environment overrides were present. Docker context `default` and
the running `postgres1` PostgreSQL 18.3 container were checked; `psql` confirmed
connectivity before testing. Application database data was not modified.

| Check | Result |
| --- | --- |
| API `dotnet restore` / `dotnet build --no-restore` | Passed |
| Ordinary backend tests, no database opt-in | Passed, 74 tests; PostgreSQL cases explicitly skipped |
| Backend tests with `DKEF_TEST_POSTGRES` | Passed, 85 tests including 11 PostgreSQL cases; temporary database removed |
| Repeated focused concurrency tests | Passed, all three cases on three further runs; `psql` confirmed no generated test databases remained |
| Migration Up/Down SQL and model snapshot review | Only new table/PK and corresponding EF model/history metadata; no content inheritance changes or seed rows |
| EF `has-pending-model-changes --context ContentsContext` | Passed; no pending model changes |
| PostgreSQL migration apply/rollback/reapply | Passed; existing `News` row survives and reapply seeds no overrides |
| PostgreSQL constraints/value/audit checks | Passed; uniqueness, 128-character bounds, Danish/multiline and SQL-like text, optional empty and default-equivalent overrides, group scoping, untracked responses, one UTC timestamp/Identity ID per batch, omitted audit data unchanged, audit survives actual contact deletion |
| PostgreSQL transaction/concurrency checks | Passed; database rejection rolls back earlier inserts/updates, response-read/pre-commit failure does not publish, concurrent first inserts/overlapping updates use the final committer's value and audit identity, disjoint changes survive |
| Frontend `bun vitest run` / `bun run build` / `bun lint` | Passed, 55 tests, Vue/TypeScript build and lint; only registry identifier validation changed, no browser-visible behavior |
| `git diff --check` and final diff review | Passed |

Existing dependency advisories remain for AngleSharp, AutoMapper, HtmlSanitizer,
Microsoft.AspNetCore.WebUtilities, and Microsoft.OpenApi; no dependency versions
were changed. Frontend tooling reports its existing runtime deprecation/
experimental-localStorage warnings. EF design-time commands also log the existing
`HostAbortedException` from `Program.cs` catching EF's intentional host abort;
commands succeed without running the post-build API migration/startup path.

## Migration/rollback handoff

Production rollout remains human-operated. Back up published overrides before
any later rollback: `Down` drops **all** `SiteTexts` rows. It leaves `Contents`
unchanged. Review scripts from `src/dkef-api` with the explicit context:

```sh
dotnet ef migrations script 20260911172703_AddAttachmentFileNames AddSiteTexts --context ContentsContext
dotnet ef migrations script AddSiteTexts 20260911172703_AddAttachmentFileNames --context ContentsContext
```

Ensure the intended deployment configuration is present for design-time commands.
No production/shared migration, API startup, deployment, push, or PR creation was
performed. HTTP authorization, error mapping, and public response filtering remain
step 04, not claimed as implemented by this persistence delivery.
