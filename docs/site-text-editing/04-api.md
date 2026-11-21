# 04 — Implement public reads and admin-only updates

**Status:** Not started

**Depends on:** [02](02-definitions-and-contract.md), [03](03-persistence.md)

**Blocks:** [05](05-frontend-state.md)

## Goal

Expose the registered text groups through one generic controller, with server-enforced permissions, plain-text validation, and all-or-nothing updates.

## Files and tasks

1. Add `src/dkef-api/Controllers/SiteTextsController.cs`, using existing `[ApiController]`/controller routing and `Program.cs` DI. Do not add an unmapped minimal endpoint under `Endpoints/`.
2. Add read/update DTOs under `Contracts/`, an update DTO following `PostObject`, and a small registry-driven validation/sanitization service under `Services/` or `Contracts/Validation/`. Implement the precise envelope, duplicate-key handling, bounds, and Danish error paths from 02.
3. Make GET explicitly anonymous. Reject unknown groups, return only registered override keys, and return `{}` for an empty registered group. Return `Cache-Control: no-store` for this initial low-traffic feature so normal reloads do not reuse stale HTTP-cached wording; frontend in-memory reuse remains separate.
4. Protect PATCH with `[Authorize(Roles = "Admin")]`. Resolve the updater's stable Identity ID from the existing authenticated claims, failing without writes if it cannot be resolved. Neither Board Member nor ordinary member status grants editing.
5. Validate group/envelope/key types and raw limits, sanitize using the feature's plain-text rules, and validate every resulting value before invoking the repository once. Do not rely solely on automatic data-annotation validation, which occurs before controller sanitization.
6. Invoke the atomic repository save and return the full group override response only after commit. Map expected validation errors to the agreed problem shape; log unexpected failures safely and retain a generic response. Do not return submitted strings or audit identities in error details.
7. Add focused automated tests for pure registry/validation/controller behavior in a minimal .NET test project if needed (none currently exists). Supplement with repeatable local HTTP/PostgreSQL checks for authorization and transaction behavior; do not make a general test-framework refactor part of this feature.

## Acceptance criteria

- [ ] Anonymous visitors can read a registered group without logging in; unknown groups are `404`.
- [ ] Anonymous updates are `401`; non-admin/Board Member updates are `403`; Admin updates succeed.
- [ ] Unknown keys, malformed payloads, excessive raw/sanitized length, required empties, invalid email, and forbidden single-line newlines fail without changing any row.
- [ ] Script/HTML submissions cannot produce executable markup or stored rich-text slots; sanitized-empty required values fail.
- [ ] Omitted keys survive, optional empty overrides round-trip, and Danish characters/multiline values round-trip correctly.
- [ ] The response contains authoritative saved overrides but no audit identity or unrelated content.
- [ ] A mixed valid/invalid batch and a persistence failure leave the entire group unchanged.

## Validation and handoff

Run backend build/tests. With confirmed disposable local resources only, verify GET/PATCH with no token, invalid token, member, Board Member, and Admin tokens. Include malformed/duplicate JSON keys, unknown groups/keys, a two-field invalid batch, boundary lengths, markup/entity examples, and two concurrent administrators editing overlapping/disjoint keys.

Provide representative responses and Danish validation errors to 05/06. API startup automatically migrates all six contexts; do not start it before checking the targets.
