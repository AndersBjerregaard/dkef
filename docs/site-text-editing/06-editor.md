# 06 — Build the reusable administrator editor

**Status:** Implemented; awaiting human review

**Depends on:** [05](05-frontend-state.md)

**Blocks:** [07](07-about.md), [08](08-home.md), [09](09-member-advantages.md)

## Goal

Create one registry-driven modal for editing any registered group, rather than separate About/Home/benefits forms.

## Files and tasks

1. Add `src/dkef-vue/src/components/SiteTextEditorModal.vue` using `BaseModal.vue`. Add a small reusable `SiteTextEditAction.vue` if it keeps page integration consistent. Use `authStore.isAdmin`; actions must react to login/logout or role changes. Frontend gating is convenience, never API authorization.
2. On each open, force a successful group load through 05, then snapshot the resolved values into a local baseline and working copy. Show a Danish loading/error/retry state. Failed loading must not initialize a saveable default-filled draft, even if public content remains readable.
3. Render developer-defined section/field order with Danish labels and associated input IDs. Single-line fields use inputs; multiline fields use textareas. Add required/format validation, length guidance, and accessible inline errors from the same metadata. Technical keys must not be editor labels.
4. Save all dirty fields in the current group together. Disable Save when loading, invalid, unchanged, not Admin, or already saving. Wire `BaseModal.isLoading` to the save operation to prevent dismissal while publication is pending; avoid duplicate submissions.
5. On success, use authoritative store values, show a Danish success notification, and close/reset the draft. A saved default-equivalent value remains an override; do not offer a misleading reset action.
6. On server validation, authorization/session, network, or persistence failure, preserve every working value and show useful Danish feedback. Map `changes.<key>` errors to fields and show a request-level fallback. Do not close the modal or publish unsaved values. Authentication loss blocks further saves; retain the draft while the modal remains open.
7. Treat Cancel, close button, Escape, and backdrop dismissal consistently: discard unsaved local values, leave published text untouched, and start fresh on reopening. If published state changes elsewhere while a draft is open, do not overwrite the working copy; keep the documented last-write-wins semantics.
8. Use semantic theme tokens and test keyboard navigation/focus restoration with the existing modal. Address `BaseModal`'s English screen-reader close label (`Close`) narrowly so the new flow is Danish; do not refactor unrelated modal behaviors.

## Acceptance criteria and tests

- [x] A registered group works without group-specific form code and presents understandable Danish sections/fields.
- [x] Load failure blocks Save and exposes retry; successful empty overrides allow editing defaults.
- [x] Local input edits never alter visible published content until a successful save.
- [x] Cancel and every close path discard changes; failed saves keep all edits and show Danish feedback.
- [x] Multi-field save is one request for one group and updates all its mounted consumers immediately.
- [x] Save cannot be duplicated or used by a non-admin; auth failures do not silently discard drafts.
- [x] Keyboard-only operation, labelled fields, inline errors, focus handling, and loading feedback work.

Add component tests for draft isolation, opening/reopening, all cancellation paths, successful save, sanitized response, invalid input, failed load/retry, failed save, no-change save, and role loss. Verify the real modal's focus/close behavior manually as well as in Vitest. Run focused tests and frontend build with Bun.

## Implementation and handoff to 07–09

`SiteTextEditorModal.vue` accepts `group` and `isOpen` props and emits `close`.
Its parent must set `isOpen` to false on close. It uses `BaseModal` without
changing its focus, transition, or dismissal behavior; the shared screen-reader
close label is now Danish (`Luk`).

For page integrations, import `SiteTextEditAction.vue` and use, for example:

```vue
<SiteTextEditAction group="about" />
```

The action owns modal visibility and reacts to `authStore.isAdmin`. Only the
trigger disappears on logout/role loss: an open editor and its draft stay mounted.
Do not wrap the action/modal in a parent Admin-only `v-if` that would destroy a
draft on session loss. API authorization remains the security boundary.

Every opening forces `refreshForEditor`; only a successful response initializes
the baseline and local draft. Closing invalidates pending initialization, so late
responses cannot recreate discarded drafts. Registry order, labels, required,
format and length metadata drive the complete form for all four current groups.
No group-specific form code or page bindings were added in this item.

Saves compare with the original opening snapshot and send changed fields together
through `saveGroup`. External published changes never replace local working
values. Omitted fields retain their latest published values; overlapping changed
fields retain the contract's last-write-wins semantics. Only the authoritative
successful response updates shared consumers. Default-equivalent edits remain
overrides, and there is no reset/delete operation.

All failure paths retain every working value. Recognized `changes.<key>` errors
appear inline until that field is edited; unrecognized/request errors get a Danish
request-level fallback. HTTP 401/403 additionally blocks further submissions until
Admin authentication changes or the editor is reopened. Login/role changes never
silently discard the open draft. Saving disables inputs, submission and every
normal dismissal path via `BaseModal.isLoading`.

## Validation record (2026-11-25)

Reused the clean, correctly scoped `feat/text-modification` branch after fetching
and checking that GitHub's current default branch (`master`) is its ancestor.
Validation uses Bun 1.4.4:

| Check | Result |
| --- | --- |
| `bun vitest run` | Passed: 114 tests across six files, including 24 editor/action tests |
| `bun run build` | Passed: Vue/TypeScript checks and Vite production build |
| `bun lint` | Passed; no unrelated changes |
| Headless Chromium (`playwright-cli`) | Passed focused desktop (1280×900) and mobile (390×844) checks |
| `git diff --check` / diff review | Passed |

Component tests use the real `BaseModal` and Headless UI, including initial focus,
Escape, backdrop, close and Cancel paths; browser checks cover native Tab/Shift+Tab
wrapping and focus restoration to the trigger after successful saving and every
dismissal path. They also cover invalid inputs, failed load/retry, failed save with
field errors, batched saves, a delayed PATCH blocking Escape/backdrop dismissal,
sanitized responses updating two consumers, fresh reopening, role loss retaining
the draft, and no horizontal overflow on mobile.

The browser check used a temporary harness served by Vite on `127.0.0.1:5187`,
non-mock `EditorCheck` mode, and Playwright-intercepted responses for a loopback API
base URL on port 5186. The successful run made seven GETs and two PATCHes (one
deliberate validation failure and one successful two-field save). Its only console
errors were the injected GET 500 and PATCH 400. This verifies frontend behavior,
not real API/database integration. The harness was removed and the dedicated
browser/server stopped. No API startup, database access, Docker changes or
migrations were needed; the already-running local Docker stack was left alone.

The existing tooling deprecation/experimental-localStorage warnings remain. Page
integrations and whole-feature validation remain steps 07–10. No push or PR was
performed, as requested; implementation approval remains a human decision.
