# 02 — Define the field registry and API contract

**Status:** Implemented and human approved (2026-11-24)

**Depends on:** [01](01-field-inventory.md)

**Blocks:** [03](03-persistence.md), [04](04-api.md), [05](05-frontend-state.md)

## Goal

Translate the approved inventory into code-owned definitions and a small, unambiguous contract used by both validation and the editor.

## Files and tasks

- Add matching package-local JSON registries, proposed paths `src/dkef-vue/src/content/siteTextDefinitions.json` and `src/dkef-api/Configuration/siteTextDefinitions.json`. Include group/key, default, Danish group/section/field labels, section order, field order, input kind, required flag, maximum length, and supported format rule.
- Add typed frontend definitions/contracts in `src/dkef-vue/src/types/siteTexts.ts` and a definition accessor in `src/dkef-vue/src/content/`. Add a typed backend registry loader under `src/dkef-api/Services/` and definition models under `Configuration/`. Keep definitions immutable at runtime.
- Ensure the backend JSON is copied to build/publish output through `src/dkef-api/dkef-api.csproj`, or embed it explicitly. Register the loader in `Program.cs`; validate definitions at startup and fail clearly for malformed registries. Do not add a public definition-management endpoint.
- Add a Vitest parity test comparing both JSON documents from the checkout, including defaults and validation metadata. Keep cross-package file access confined to tests: runtime builds must remain package-local. Verify both existing Docker contexts can package their own definitions without workflow changes.
- Reject duplicate identifiers, unknown format/input kinds, invalid limits/order metadata, and defaults that violate the field rules. Specify case-sensitive, ordinal group/key matching and no automatic identifier normalization.

## Contract to implement

Public `GET /SiteTexts/{group}` and successful admin `PATCH /SiteTexts/{group}` return the same shape:

```json
{
  "group": "home",
  "overrides": {
    "hero.title": "Dit nye netværk i elbranchen"
  }
}
```

An existing, registered group with no stored rows returns `200` with `"overrides": {}`. Unknown groups return `404`, not an empty success. Public responses omit `UpdatedBy`, timestamps, and unregistered/retired keys. Frontend defaults are bundled locally; no read request is needed to display readable initial content.

PATCH accepts:

```json
{
  "changes": {
    "hero.title": "Dit nye netværk i elbranchen",
    "hero.description": "En opdateret beskrivelse."
  }
}
```

- `changes` is a required, nonempty object of registered keys to string values. Reject nulls, nonstrings, duplicate JSON keys, unknown envelope members, and unknown keys; do not silently drop invalid input. Bound request size/change count using the approved registry rather than accepting an unbounded dictionary.
- Omitted fields stay unchanged. Empty strings are valid only for optional fields and remain stored overrides. No delete/reset/null operation is exposed initially. Saving a value equal to a default still creates/retains an override.
- Normalize CRLF/CR to LF. Reject newlines in single-line fields. Preserve multiline line breaks and intentional spaces; required whitespace-only text fails. Measure maximum length in UTF-16 code units consistently in C# and JavaScript.
- Follow `PostObject.Sanitize(HtmlSanitizer)` conventions, but do not confuse the existing HTML allowlist with plain-text validation. Strip markup using a feature-specific sanitizer configuration without altering other DTOs, then revalidate requiredness, format, and length on the resulting plain text. Reject oversized raw values as well. Define and test entity handling so Danish letters, `&`, and literal comparison characters are not double-encoded. Render exclusively through interpolation.
- All keys are validated before writing. Invalid requests return `400` `ValidationProblemDetails` with Danish field errors keyed as `changes.<semantic-key>`; malformed envelopes use a request-level error. Unexpected persistence failures return a generic problem response without SQL or submitted content.
- Missing/invalid credentials receive `401`; authenticated users without `Admin` receive `403`. Audit identity is derived from the authenticated Identity ID claim, not request data; inspect the existing JWT issuer's claim mapping before choosing the accessor.
- Simultaneous edits use last-committed-write-wins for the same submitted key. Disjoint-key saves preserve both sets of changes. Atomic upserts must also handle two administrators creating the same previously missing override. No conflict-detection promise is made.
- Successful PATCH returns the group's full current override set from the transactional save, including sanitized values. Publish only after commit. One group per request; no cross-group atomicity.

## Acceptance criteria and validation

- [x] Approved defaults and metadata are represented identically in both packages.
- [x] Registry tests cover duplicate IDs, invalid definitions, default validity, and cross-package parity.
- [x] Request/response types and validation error shape cover missing, empty, and malformed cases; unauthorized HTTP outcomes are specified below for 04.
- [x] Sanitization, whitespace, UTF-16 length, and no-reset value semantics have concrete executable examples. Simultaneous-edit semantics have explicit integration scenarios below for 03/04; no persistence implementation is claimed here.
- [x] `dotnet build`/publish and frontend type-check/build find definitions without repository-root runtime files.

Pass the stable contract and registry accessors to 03–06. If approval in 01 changes any baseline behavior, update this contract before implementing endpoints or clients.

## Human approval record

- Approved by the requesting human on **2026-11-24**.
- Reviewed implementation: commit `e7a6f33` (`feat: define site-text registry and API contract`).
- Approval covers item 02's implementation, contract, validation results, and
  documented handoff. Runtime authorization and concurrent database integration
  checks remain in 03/04; this approval does not mark those later items complete.

## Implementation and handoff

The [inventory approval record](field-inventory.md#approval-record) accepts all
73 fields and baseline decisions. Both packages contain identical JSON documents:
groups contain sections, sections contain fields, and the containing group ID
plus field key is the full identifier. Section IDs are developer-owned editor
metadata, not a second persistence key. Orders are unique positive 32-bit integers
within their containing list; accessors sort by explicit order. Keys are unique
across **all** sections in a group. Identifiers use lowercase ASCII semantic
segments separated by `.`/`-`; lookups use exact ordinal equality, with no trimming
or normalization. New editable fields require updates to both JSON files and
parity validation, not a database schema change.

### Reusable entry points

- Frontend: `src/content/siteTexts.ts` exports `siteTextRegistry`,
  `getSiteTextGroup`, `getSiteTextFields`, and `getSiteTextDefaults`. Definitions
  are readonly and deeply frozen; defaults need no network. Unknown groups return
  `undefined`. `siteTextRules.ts` supplies normalization and field checks for editor
  usability, **not** a browser HTML sanitizer. `types/siteTexts.ts` defines read,
  patch, validation-problem, and error-status contracts.
- Backend: `SiteTextRegistry.Load`/`Parse`, `Groups`, `FindGroup`, and `GetFields`
  expose immutable records/arrays and ordinal frozen dictionaries. `FindGroup`
  returns null for unknown groups; callers must check it before `GetFields`.
  `Program.cs` eagerly loads/validates the package-local output JSON before any
  external services or migrations. Missing/malformed definitions fail startup
  with an explicit registry error. There is no registry-management endpoint.
- `SiteTextPatchContract.Validate(utf8Json, registry, group)` implements pure wire
  and value checks. It returns **either** a complete sanitized
  `SiteTextPatchRequest` **or** errors, never a partial request. The controller in
  04 must use this parser, not ordinary dictionary model binding (which loses
  duplicate-property evidence). `SiteTextGroupResponse` is the GET/PATCH shape.
- `SiteTextPatchValidationResult.ToProblemDetails()` supplies Danish
  `ValidationProblemDetails` with status 400. Neither errors nor registry code
  performs a database write or grants authorization.

### Transport, errors, and authorization

For a registered group with `N` fields, PATCH accepts 1–`N` changes. The maximum
UTF-8 body size is `1024 + sum(6 * (key.Length + maxLength) + 8)` bytes across
the group's fields. This allows worst-case JSON `\uXXXX` escaping of every UTF-16
unit plus envelope/punctuation and bounded whitespace. Arbitrarily pretty-printed
or padded JSON need not be accepted. The controller must enforce this bound on
the **incoming stream**, including chunked requests, before buffering; checking
only `Content-Length` is insufficient. Oversized transport bodies may be rejected
with 413; the pure validator also rejects an already-buffered oversized body with
a request-level 400. Raw per-value limits always apply before sanitization or
newline normalization. JSON depth is bounded to four; comments and trailing
commas are not accepted. Duplicate properties include escaped-equivalent names
such as `hero.title` and `hero.\u0074itle`.

| Outcome | HTTP / shape required in 04 |
| --- | --- |
| Registered group, no rows | 200 `{ "group": "home", "overrides": {} }` |
| Successful GET/PATCH | 200 full current registered override set; no defaults, retired keys, timestamps, or audit identity |
| Unknown/case-mismatched group | 404, never an empty success |
| Missing/invalid credentials | 401 from Bearer authentication |
| Authenticated non-Admin, including Board Member | 403; do not invoke save |
| Malformed/missing/empty/duplicate envelope | 400 validation problem under `errors.request` |
| Unknown key, null/nonstring value, field-rule violation | 400 under `errors["changes.<semantic-key>"]` |
| Unexpected persistence failure | Generic problem response (500), no SQL, submitted content, or identity in public details |

Example field error (additional standard ProblemDetails properties are allowed):

```json
{
  "status": 400,
  "title": "En eller flere tekster er ugyldige.",
  "errors": { "changes.hero.title": ["Feltet skal udfyldes."] }
}
```

Keep GET anonymous and `Cache-Control: no-store`; protect PATCH with the existing
`Admin` role before reading/validating/writing the body. `JwtService` emits the
Identity ID as `ClaimTypes.NameIdentifier` (serialized `nameid`) and **email** as
`sub`. Default inbound mapping maps **both** to `NameIdentifier`.
`SiteTextAuditIdentity.FindIdentityId(User)` distinguishes the mapped `nameid`
using the handler's original-short-name property; do not use `FindFirst`, `sub`,
email, or request data as the audit ID. It fails closed for anonymous, missing,
ambiguous, or unmapped claims. Tests issue and validate an actual signed token
through the current `JsonWebTokenHandler` mapping, with email deliberately first.
If the issuer/mapping changes, update this accessor and tests together. In 04,
an authenticated Admin without a usable audit ID must receive 403 without writes.

### Plain text and executable value examples

The server calls the `PostObject.Sanitize(HtmlSanitizer)` override with a dedicated
feature configuration: no allowed tags, attributes, or CSS properties, with child
text retained. It does not modify the application's rich-text sanitizer. Stripped
tag contents (including script contents) are inert text, never executable markup.
The sanitized HTML serializer result is entity-decoded **exactly once**, then
normalized and revalidated for requiredness, format, and UTF-16 length. This is
not a repeated decode/unescape loop. Encoded literal markup can therefore remain
literal text: **all consumers must render exclusively through interpolation**,
never `v-html`, raw HTML, URLs, or script/style contexts. JSON transport escaping
of `<` or `&` is harmless; decoded string values are authoritative.

| Submitted text | Stored/returned string or result |
| --- | --- |
| `<b>Æble</b> &amp; øl` | `Æble & øl` |
| `2 < 3 & 4 > 1` | Same literal comparison characters and ampersand |
| `&#198; &oslash; &aring;` | `Æ ø å` |
| `&lt;b&gt;tekst&lt;/b&gt;` | Literal `<b>tekst</b>`, displayed as text, not bold |
| `&amp;lt;` | Literal `&lt;`, not double-decoded to `<` |
| `<b></b>` in a required field | 400 after sanitization removes all content |
| Two leading/trailing spaces | Retained, not trimmed |
| `A\r\n\rB` in multiline text | `A\n\nB`; component-owned presentation preserves line breaks later |
| CR/LF, NEL, U+2028, or U+2029 in single-line text | 400; CR/CRLF first normalize to LF |
| `😀😀` with maximum 4 / maximum 3 | Accepted / rejected: four UTF-16 units, not two |
| `A\r\nB` with maximum 3 | Rejected: four raw units even though normalization yields three |

Required whitespace uses an explicit shared union of .NET/ECMAScript whitespace
(including NEL and BOM), so whitespace-only input fails identically. Format rules
are `plain`, `email`, and `phone` only. Email follows the API's `EmailAddressAttribute`
single-`@` convention plus a bare-mailbox restriction: no whitespace, display name,
angle brackets, comma, semicolon, colon, or quote; a domain dot is not required
(`info@localhost` is valid, `Email: info@dkef.dk` is not). Phone allows ASCII digits,
ordinary spaces, `+`, `-`, and parentheses, with at least one digit. Neither rule
changes contact destinations or business behavior. Empty optional values skip
format validation but remain real stored values; all 73 initial fields are required.

The same 20 rule/normalization examples in `tests/site-text-value-cases.json` run
in Vitest and .NET. Backend tests additionally cover actual sanitization/entities,
raw bounds, envelopes, duplicate keys, post-sanitization validation, response
serialization, no-partial-request behavior, and the audit claim mapping.

### Transaction scenarios for 03/04

The pure contract tests confirm that omitted keys are absent from validated
changes, optional `""` remains a change, default-equivalent text remains a change,
and nulls fail. The repository must **not** reinterpret these values as deletes.
Run the following PostgreSQL integration scenarios in 03/04, when persistence
and endpoints exist; dictionary-merge simulations would not verify atomic upserts:

1. A submits `{ "hero.title": "A" }`; B submits
   `{ "hero.description": "B" }`. After both commit, both overrides exist,
   regardless of who commits last. Never replace a complete group with a stale snapshot.
2. Both submit `hero.title`, initially missing, with `"A"` and `"B"`.
   Atomic unique-key upserts must let both saves complete; the last committed
   value wins without a uniqueness failure or duplicate row. Repeat with an existing row.
3. One batch contains valid `hero.title` and invalid `hero.description`.
   Nothing is written; an injected persistence failure also rolls back the full batch.
4. Saving text equal to the default still creates/retains a row. A future optional
   field saved as `""` round-trips empty, while an omitted field remains unchanged.
   There is no null/delete/reset operation and no cross-group transaction.
5. A successful response is read inside the saving transaction and returned/published
   **after commit**. It contains the group's full registered override set from that
   transaction, including sanitized values. A later concurrent commit may of course
   make that snapshot stale; no conflict detection or cross-browser sync is promised.

### Validation record

Validated on 2026-11-24, on the existing `feat/text-modification` task branch,
after fetching and verifying its base against GitHub's default branch (`master`).
Tools: Bun 1.4.4 and the environment's .NET SDK 10.0.400 (Windows-hosted tool bridge).

| Check | Result |
| --- | --- |
| API `dotnet restore` / `dotnet build --no-restore` | Passed |
| Root `dotnet test tests/dkef-api.Tests/dkef-api.Tests.csproj --no-restore` | Passed, 70 tests; no API startup/database access |
| `dotnet publish src/dkef-api/dkef-api.csproj -c Release -o src/dkef-api/bin/site-text-contract-publish --no-restore` | Passed; output JSON byte-matches both source registries and Debug build output |
| Frontend `bun run build` | Passed, including Vue/TypeScript checks |
| Frontend `bun lint` | Passed, no unrelated source changes |
| Frontend `bun vitest run` | Passed, 52 tests (51 site-text tests plus existing smoke test) |
| Frontend `bun build ./src/content/siteTexts.ts --target browser --outfile /tmp/opencode/site-text-registry.js` | Passed; independent registry bundle includes defaults with no sibling/root runtime imports |
| Docker-context review | Both workflows still use package-local contexts; `COPY . .` includes each local JSON, neither `.dockerignore` excludes it, API publish copies it, frontend imports it locally. No workflow/Dockerfile changes or image builds needed |
| Diff/Markdown link review | Passed; generated outputs remain ignored/outside commits |

Existing dependency advisories were reported for AngleSharp, AutoMapper,
HtmlSanitizer, Microsoft.AspNetCore.WebUtilities, and Microsoft.OpenApi; no existing
dependency versions were changed. Release publish also reports the existing
unused `ImageCleanupController._cleanupService` parameter. Frontend tooling emits
runtime deprecation/experimental-localStorage warnings, with passing checks.

No browser flow is changed in this item, so no additional browser validation was
needed. No API startup, database access, migrations, infrastructure changes, push,
or PR creation was performed. Authorization HTTP statuses and actual concurrent
database writes are specified above, not claimed as integration-tested before
03/04 exist.
