# 04 — Implement public reads and admin-only updates

**Status:** Implemented; awaiting human review

**Depends on:** [02](02-definitions-and-contract.md), [03](03-persistence.md)

**Blocks:** [05](05-frontend-state.md)

## Goal

Expose the registered text groups through one generic controller, with server-enforced permissions, plain-text validation, and all-or-nothing updates.

## Files and tasks

1. Add `src/dkef-api/Controllers/SiteTextsController.cs`, using existing `[ApiController]`/controller routing and `Program.cs` DI. Do not add an unmapped minimal endpoint under `Endpoints/`.
2. Reuse 02's `SiteTextGroupResponse`, `SiteTextPatchRequest` (`PostObject`), `SiteTextPatchContract`, and `SiteTextValueRules`. Wire the pure parser into bounded request-stream handling rather than ordinary dictionary model binding. Preserve its envelope, duplicate-key handling, raw/sanitized bounds, and Danish error paths; do not sanitize validated changes a second time.
3. Make GET explicitly anonymous. Reject unknown groups, return only registered override keys, and return `{}` for an empty registered group. Return `Cache-Control: no-store` for this initial low-traffic feature so normal reloads do not reuse stale HTTP-cached wording; frontend in-memory reuse remains separate.
4. Protect PATCH with `[Authorize(Roles = "Admin")]`. Resolve the updater's stable Identity ID with `SiteTextAuditIdentity.FindIdentityId(User)` from 02; return 403 without writes if it cannot be resolved. Do not use `FindFirst(NameIdentifier)` (the existing issuer maps both email `sub` and Identity `nameid` to that claim type). Neither Board Member nor ordinary member status grants editing.
5. Validate group/envelope/key types and raw limits, sanitize using the feature's plain-text rules, and validate every resulting value before invoking the repository once. Do not rely solely on automatic data-annotation validation, which occurs before controller sanitization.
6. Invoke the atomic repository save and return the full group override response only after commit. Map expected validation errors to the agreed problem shape; log unexpected failures safely and retain a generic response. Do not return submitted strings or audit identities in error details.
7. Add focused automated tests for pure registry/validation/controller behavior in a minimal .NET test project if needed (none currently exists). Supplement with repeatable local HTTP/PostgreSQL checks for authorization and transaction behavior; do not make a general test-framework refactor part of this feature.

## Acceptance criteria

- [x] Anonymous visitors can read a registered group without logging in; unknown groups are `404`.
- [x] Anonymous updates are `401`; non-admin/Board Member updates are `403`; Admin updates succeed.
- [x] Unknown keys, malformed payloads, excessive raw/sanitized length, required empties, invalid email, and forbidden single-line newlines fail without changing any row.
- [x] Script/HTML submissions cannot produce executable markup or stored rich-text slots; sanitized-empty required values fail.
- [x] Omitted keys survive, optional empty overrides round-trip, and Danish characters/multiline values round-trip correctly.
- [x] The response contains authoritative saved overrides but no audit identity or unrelated content.
- [x] A mixed valid/invalid batch and a persistence failure leave the entire group unchanged.

## Validation and handoff

Run backend build/tests. With confirmed disposable local resources only, verify GET/PATCH with no token, invalid token, member, Board Member, and Admin tokens. Include malformed/duplicate JSON keys, unknown groups/keys, a two-field invalid batch, boundary lengths, markup/entity examples, and two concurrent administrators editing overlapping/disjoint keys.

Provide representative responses and Danish validation errors to 05/06. API startup automatically migrates all six contexts; do not start it before checking the targets.

## Implementation and handoff to 05/06

`Controllers/SiteTextsController.cs` is discovered by the existing
`AddControllers` / `MapControllers` setup. It uses the registry singleton and scoped
repository already registered in `Program.cs`; no additional endpoint mapping,
dependency, schema change, or DI registration was needed.

- `GET /SiteTexts/{group}` is explicitly anonymous, ordinal/case-sensitive at the
  registry boundary, and returns `Cache-Control: no-store`, including failed reads.
  Unknown groups return 404. Registered empty groups return
  `{"group":"home","overrides":{}}`. Stored retired keys, other groups, defaults,
  and audit metadata are never included in successful GET/PATCH responses.
- PATCH requires the exact `Admin` role through Bearer authorization before the
  controller runs. An authenticated Admin without a usable mapped Identity ID
  receives 403 before body reading or persistence. Identity comes exclusively from
  `SiteTextAuditIdentity`, never `sub`, email, or submitted data.
- PATCH bounds both `Content-Length` and actual stream consumption using
  `SiteTextPatchContract.MaxRequestBytes`. The reader consumes at most the bound
  plus one sentinel byte and does not require seeking, including chunked bodies.
  The bound is inclusive. Oversized transport returns 413, with the same validation
  problem structure and `errors.request: ["Anmodningen er for stor."]`.
- No dictionary body binding is used. The existing parser checks the complete
  envelope, duplicate keys (including escaped equivalents), raw lengths, and all
  post-sanitization rules. Its `PostObject` sanitization runs exactly once. Only a
  fully valid request reaches the repository's single atomic save invocation.
- Successful PATCH returns the full filtered transactional snapshot **after
  commit**. Omitted keys are untouched, optional empty values are stored, and
  default-equivalent values remain overrides. There is no reset/delete operation.
  Simultaneous disjoint-key edits survive; overlapping keys use last commit wins,
  including concurrent first inserts. No conflict/ETag mechanism is added.
- Unexpected read/write failures return a generic 500 problem with Danish title
  `Teksterne kunne ikke hentes eller gemmes. Prøv igen senere.` Controller logs
  include only exception type and request trace, not exception messages/objects,
  SQL, submitted text, or updater identities. Client cancellation is propagated.

Representative successful PATCH, submitting
`{"changes":{"hero.title":"<b>Æble</b> &amp; øl"}}` to an empty `home` group:

```json
{"group":"home","overrides":{"hero.title":"Æble & øl"}}
```

Representative invalid required field (400, `application/problem+json`):

```json
{
  "status": 400,
  "title": "En eller flere tekster er ugyldige.",
  "errors": { "changes.hero.title": ["Feltet skal udfyldes."] }
}
```

Malformed/duplicate envelopes use `errors.request` with
`Anmodningen skal indeholde ét ikke-tomt changes-objekt med entydige felter.`
Unknown fields use `errors["changes.<key>"]` with
`Feltet findes ikke i denne tekstgruppe.` Standard problem properties such as
`type` and `traceId` may also be present. Clients must treat 401/403/404/413/500 as
failed saves and retain local edits, not substitute a successful empty load.
Encoded literal markup can remain literal text, e.g. `&lt;b&gt;tekst&lt;/b&gt;`
returns `<b>tekst</b>`, while `&amp;lt;` returns `&lt;`. Render **only through
interpolation**, never `v-html`, and do not sanitize/decode the response again.

## Repeatable validation

Ordinary tests remain database-free:

```sh
dotnet restore tests/dkef-api.Tests/dkef-api.Tests.csproj
dotnet build src/dkef-api/dkef-api.csproj --no-restore
dotnet test tests/dkef-api.Tests/dkef-api.Tests.csproj --no-restore
```

For real HTTP/PostgreSQL checks, follow [03's opt-in instructions](03-persistence.md#repeatable-postgresql-validation)
to set `DKEF_TEST_POSTGRES` securely to a confirmed loopback development PostgreSQL
maintenance connection. With Windows-hosted `dotnet.exe` under WSL, include that
variable in `WSLENV`. Run the same test command. To focus/repeat HTTP concurrency:

```sh
dotnet test tests/dkef-api.Tests/dkef-api.Tests.csproj --no-build --no-restore --filter FullyQualifiedName~ConcurrentAdministrators
```

`SiteTextControllerTests.cs` tests early rejection without opening a connection,
nonseekable/missing or false-length bodies, bounded reading, transport exceptions,
parser error mapping, and cancellation. `SiteTextHttpTests.cs` uses real Kestrel on
an ephemeral loopback port, actual MVC/Bearer authorization and claim mapping, the
real controller/repository, and a separate uniquely generated test database. It
uses in-memory test-only JWT configuration and mirrors `Program.cs` token validation;
keep them aligned if authentication changes. It never invokes application startup,
seeds users, or uses application connection settings. The existing PostgreSQL
fixture migrates only its generated database and drops it on teardown. A test-only
registry copy includes a future optional slot; the approved production inventory
is unchanged. There are no new test packages or fake transaction providers.

### Validation record (2026-11-25)

Reused the clean `feat/text-modification` task branch after fetching and verifying
its base against GitHub's current default branch (`master`). Confirmed Docker
context `default`, local PostgreSQL 18.3 container `postgres1` on port 5432, and
local Development API/MinIO connection targets. `appsettings.Local.json` contains
no connection overrides and no environment connection overrides were present.
Only generated test databases were modified; the application's `dkef` database
and its six-context startup migrations were not run.

| Check | Result |
| --- | --- |
| Backend restore / API build | Passed with .NET SDK 10.0.400 (Windows-hosted tool bridge) |
| Ordinary tests without database opt-in | Passed, 86 tests; 19 integration test methods explicitly skipped (theory cases expand when enabled) |
| All tests with local PostgreSQL opt-in | Passed, 128 tests: 86 database-free, 11 repository/PostgreSQL, and 31 HTTP/PostgreSQL cases |
| HTTP concurrent first insert / existing overlap / disjoint saves | All three passed, plus three additional focused runs; overlapping writes observed blocked in PostgreSQL while disjoint save completed before first commit |
| Failure/publication checks | Invalid mixed batch leaves rows unchanged; database constraint failure rolls back an earlier write and returns generic 500; no HTTP success before commit |
| Cleanup | PostgreSQL query confirmed no generated test databases remained |
| Diff review / `git diff --check` | Passed |

Existing dependency advisories remain for AngleSharp, AutoMapper, HtmlSanitizer,
Microsoft.AspNetCore.WebUtilities, and Microsoft.OpenApi; no dependency versions
were changed. No frontend code or visible browser flow changed, so frontend build/
browser checks were not needed. No application startup, shared/production writes,
deployment, push, or PR creation was performed. Implementation is committed locally
for human review, not marked human-approved.
