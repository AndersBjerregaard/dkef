# 05 — Build the shared frontend text mechanism

**Status:** Not started

**Depends on:** [02](02-definitions-and-contract.md), [04](04-api.md)

**Blocks:** [06](06-editor.md)

## Goal

Provide one reactive, in-memory group cache that displays defaults reliably and publishes successful saves to every mounted consumer of the same group.

## Files and tasks

1. Add group URL builders to `src/dkef-vue/src/services/urlservice.ts`; encode the registered group path segment. Keep new endpoint paths out of components/stores. Define consistent behavior for the existing mock-like `VITE_MODE=Development`; do not fabricate a successful empty response on unavailable API loads.
2. Add a typed `patch` wrapper to `src/dkef-vue/src/services/apiservice.ts`, preserving request configuration/token refresh behavior. Use `skipAuth: true` for public GET and normal authenticated config for PATCH. Public GET failures must not trigger login prompts/token refresh; narrowly respect `skipAuth` in the response interceptor if required, with regression tests for authenticated refresh.
3. Add a setup-style `src/dkef-vue/src/stores/siteTextStore.ts` and, if useful, a thin `src/dkef-vue/src/composables/useSiteTexts.ts`. Keep the store non-persisted despite the app's persisted-state plugin.
4. Track per group: overrides, pending load promise, explicit load status (`idle`, `loading`, `loaded`, `error`), load error, and save status. Fetch one group per request and deduplicate concurrent loads. Failed loads can be retried; do not treat them as successful empty groups.
5. Resolve registered values by key presence, not truthiness: an optional `""` overrides its default. Expose computed/reactive merged values with defaults before loading and when a first load fails. On a later refresh failure, retain known published overrides while marking the failed refresh; do not erase them with `{}`.
6. Require a successful current editor load before saves. Provide a forced group refresh for opening the editor, separate from ordinary cached display reuse. A failed editor refresh blocks saving even if an older public cache exists.
7. Save only changed registered key/value pairs for one group. Do not mutate published state optimistically. Replace that group's overrides with the authoritative PATCH response on success; keep published state and caller drafts unchanged on errors.
8. Prevent stale GET responses from overwriting a newer save, using a modest request sequence/generation guard or serialization. Serialize/disable overlapping same-group saves within this browser; no realtime or persistent cache infrastructure.

## Acceptance criteria and tests

- [ ] Missing overrides and initial public fetch failures display bundled defaults; an empty optional override is not replaced by a default.
- [ ] Two consumers of one group share a single pending GET and reactive published values.
- [ ] Successful empty loads and failed loads are distinguishable; retry works and editor failures block saving.
- [ ] PATCH sends changed keys only, updates the complete group after success, and does not mutate state on failure.
- [ ] A delayed GET cannot undo a successful local save; local drafts are not live store references.
- [ ] Public reads do not send credentials or cause login side effects; authenticated update refresh behavior remains intact.
- [ ] The store is not persisted in local/session storage and fetches groups rather than individual fields.

Use Vitest with mocked API responses, including late responses, server-sanitized values, malformed response handling, and load/save rejection. Run frontend type-check/build and relevant tests with Bun. API-backed manual checks need non-mock mode pointed at the approved local API, not shared resources.
