# 06 — Build the reusable administrator editor

**Status:** Not started

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

- [ ] A registered group works without group-specific form code and presents understandable Danish sections/fields.
- [ ] Load failure blocks Save and exposes retry; successful empty overrides allow editing defaults.
- [ ] Local input edits never alter visible published content until a successful save.
- [ ] Cancel and every close path discard changes; failed saves keep all edits and show Danish feedback.
- [ ] Multi-field save is one request for one group and updates all its mounted consumers immediately.
- [ ] Save cannot be duplicated or used by a non-admin; auth failures do not silently discard drafts.
- [ ] Keyboard-only operation, labelled fields, inline errors, focus handling, and loading feedback work.

Add component tests for draft isolation, opening/reopening, all cancellation paths, successful save, sanitized response, invalid input, failed load/retry, failed save, no-change save, and role loss. Verify the real modal's focus/close behavior manually as well as in Vitest. Run focused tests and frontend build with Bun.
