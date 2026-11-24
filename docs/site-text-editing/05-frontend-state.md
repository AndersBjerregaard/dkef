# 05 — Build the shared frontend text mechanism

**Status:** Implemented and human approved (2026-11-25)

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

- [x] Missing overrides and initial public fetch failures display bundled defaults; an empty optional override is not replaced by a default.
- [x] Two consumers of one group share a single pending GET and reactive published values.
- [x] Successful empty loads and failed loads are distinguishable; retry works and editor failures block saving.
- [x] PATCH sends changed keys only, updates the complete group after success, and does not mutate state on failure.
- [x] A delayed GET cannot undo a successful local save; local drafts are not live store references.
- [x] Public reads do not send credentials or cause login side effects; authenticated update refresh behavior remains intact.
- [x] The store is not persisted in local/session storage and fetches groups rather than individual fields.

Use Vitest with mocked API responses, including late responses, server-sanitized values, malformed response handling, and load/save rejection. Run frontend type-check/build and relevant tests with Bun. API-backed manual checks need non-mock mode pointed at the approved local API, not shared resources.

## Human approval record

- Approved by the requesting human on **2026-11-25**.
- Reviewed implementation: commit `bc9ee61` (`feat: add shared frontend site-text state`).
- Approval covers item 05's shared frontend state, API-client support, validation
  results, and documented handoff.
- This approval does not authorize deployment or mark items 06–10's editor,
  component integrations, or full-feature verification complete.

## Implementation and handoff to 06

`src/stores/siteTextStore.ts` provides one setup-style Pinia store per application,
with no persistence option. The app-wide persisted-state plugin remains opt-in.
No composable, new dependency, editor, or component binding is needed in this item.

- `texts[group][key]` is the reactive, readonly merged display value. Defaults are
  immediately available for every registered group. Merging uses key presence,
  preserving `""`; there is no client sanitization or entity decoding.
- `groups[group]` exposes readonly `overrides`, `loadStatus`, `loadError`,
  `saveStatus`, `saveError`, and `editorReady`. Load states are `idle`, `loading`,
  `loaded`, and `error`; save states are `idle`, `saving`, `saved`, and `error`.
  Original request errors, including Axios validation problems, are retained and
  rethrown for the editor to present in Danish. Display callers should catch load
  rejections while continuing to render `texts`.
- `loadGroup(group)` reuses a loaded group or a pending GET. Failed loads can be
  retried. `refreshGroup(group)` explicitly bypasses display caching. Both retain
  known published overrides on failure; neither authorizes editing.
- `refreshForEditor(group)` always starts a new GET, including when a display GET
  is pending. It immediately blocks saving, and only its current successful result
  enables editing. Superseded editor requests reject instead of initializing a
  saveable stale draft. A failed refresh blocks saving even with older cached text.
- Keep the returned editor values as the **original baseline**, and edit a separate
  shallow copy. `createDraft(group)` also returns a detached copy; it does not fetch
  or authorize saving. `saveGroup(group, draft, original)` compares with the editor's
  original snapshot, not potentially newer shared state, and submits only changed
  registered keys. Unchanged drafts send no request. Successful responses replace
  the entire override set, including server-sanitized strings; failed saves leave
  both published values and caller-owned snapshots untouched.
- Same-group overlapping saves reject; other groups are independent. Per-group
  generations prevent old GET successes/failures from undoing a newer save or
  refresh. Promises/generations remain private, in-memory coordination only.
- Response envelopes must have the requested group and an override object of
  registered string-valued keys. Malformed GET/PATCH data rejects rather than
  masquerading as an empty load or publishing partial state. Field-value usability
  validation remains in the registry rules/editor; authoritative validation and
  sanitization remain on the API. All initial fields are required; the empty-string
  state-resolution test exercises forward-compatible optional-value semantics.

Typical editor initialization/save (error/loading presentation belongs to 06):

```ts
const original = await store.refreshForEditor(group)
const draft = { ...original }
// Edit draft locally; cancellation simply discards it.
await store.saveGroup(group, draft, original)
```

URL construction stays in `urlservice.ts`, with an encoded group segment for GET
and PATCH. Both explicitly throw in mock-like `VITE_MODE=Development`; the store
records a failed load and keeps defaults, never a fabricated successful empty
group. Non-development modes use the configured API base URL.

`apiservice.patch<T>` preserves Axios body/config handling and normal Bearer/refresh
behavior. Public site-text GET uses `skipAuth: true` and `withCredentials: false`.
The response interceptor now rejects `skipAuth` failures without token refresh,
session expiration, or login prompts. Authenticated requests still refresh, queue,
retry, and prompt on refresh failure as before.

## Validation record (2026-11-25)

Reused the clean, correctly scoped `feat/text-modification` branch after fetching
and verifying that GitHub's current default branch (`master`) is its ancestor.
Frontend checks ran with Bun 1.4.4:

| Check | Result |
| --- | --- |
| `bun vitest run` | Passed, 90 tests across five files; 35 new store/service tests |
| `bun run build` | Passed, including Vue/TypeScript checks and Vite production build |
| `bun lint` | Passed; no unrelated source changes |
| Headless Chromium with `playwright-cli` | Passed the focused store/API-client browser check described below |
| `git diff --check` / diff review | Passed |

The browser check used an isolated Vite server on `127.0.0.1:5185`, a non-mock
mode, and **Playwright-intercepted** responses for a loopback API base URL on port
5186. It was not a real API/database integration test. Two temporary reactive
display consumers retained defaults after one deduplicated anonymous GET returned
401, with zero login prompts. A forced editor GET then succeeded, followed by one
changed-key PATCH; both consumers displayed the authoritative response while the
draft remained unchanged and no `siteTexts` local-storage entry appeared. The
network log contained only the expected site-text GET 401, GET 200, and PATCH 200;
the deliberately injected 401 was the only console error. The browser session and
test server were closed. No existing layout or responsive UI was changed.

Tooling still emits the existing `module.register()` deprecation and experimental
localStorage warnings. No application API startup, database access, migrations,
Docker changes, deployment, push, or PR creation was performed. The running local
Docker stack was not needed for this isolated frontend work. Item 06 and page
integrations remain unimplemented; human approval of this item is recorded above.
