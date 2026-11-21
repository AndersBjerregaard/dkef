# 03 — Add site-text override persistence

**Status:** Not started

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

- [ ] The migration creates one separate `SiteTexts` table and no changes to `Contents` inheritance.
- [ ] Duplicate group/key rows are impossible; reads and writes are group-scoped.
- [ ] Multiple valid changes commit together; a database failure rolls the batch back.
- [ ] Concurrent first inserts and overlapping updates follow the documented policy; omitted keys survive.
- [ ] Audit metadata cannot be supplied/spoofed by the caller and is not lost on user deletion.
- [ ] Defaults remain code-owned; a new field/group needs no schema change.

## Validation and handoff

Run `dotnet restore` and `dotnet build`. Review migration SQL without applying it to shared resources. Before database tests or API startup, confirm all configured contexts, including optional `appsettings.Local.json`, point to disposable local resources. Verify apply/rollback on that database, uniqueness, optional empty text, audit timestamps/IDs, batch rollback, and concurrent upsert behavior. Use PostgreSQL, not an in-memory provider, for transaction/constraint checks.

Hand the migration and repository API to 04. Record migration/rollback requirements for the later human-operated rollout; do not apply anything to production.
