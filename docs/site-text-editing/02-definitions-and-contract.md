# 02 — Define the field registry and API contract

**Status:** Not started

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

- [ ] Approved defaults and metadata are represented identically in both packages.
- [ ] Registry tests cover duplicate IDs, invalid definitions, default validity, and cross-package parity.
- [ ] Request/response types and validation error shape cover missing, empty, malformed, and unauthorized cases.
- [ ] Sanitization, whitespace, length measurement, simultaneous edits, and no-reset semantics are documented and tested with concrete examples.
- [ ] `dotnet build`/publish and frontend type-check/build find definitions without repository-root runtime files.

Pass the stable contract and registry accessors to 03–06. If approval in 01 changes any baseline behavior, update this contract before implementing endpoints or clients.
