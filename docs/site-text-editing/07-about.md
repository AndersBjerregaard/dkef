# 07 — Integrate editable About text

**Status:** Implemented; awaiting human review

**Depends on:** [06](06-editor.md) and the approved inventory in [01](01-field-inventory.md)

**Blocks:** [10](10-verification-and-handoff.md)

## Goal and files

Connect `src/dkef-vue/src/components/AboutComponent.vue` to the `about` group without changing the page's structure, images, contact form, or factual numeric values.

## Tasks

1. Bind each approved heading, introduction, statistic label, secretariat display field, and contact heading to reactive registered values. Use Vue interpolation, never `v-html`. Leave deliberately excluded text literal/code-owned.
2. Add the shared Admin-only action labelled `Rediger tekster` for the About group, opening 06. Do not create an About-specific persistence operation or editor.
3. Keep `+550`, `5`, and `1903` outside the registry as agreed in 01. Keep both images, element types, section order, and `ContactFormularComponent` behavior unchanged.
4. Keep approved phone/email presentation prefixes code-owned. Changing display contact text must not change email delivery configuration or form destinations.
5. Preserve line breaks for approved multiline prose using narrowly scoped component-owned presentation rules (for example `white-space: pre-line`). Do not alter arbitrary markup or rearrange layout to accommodate unlimited text.
6. Exercise approved maximum lengths against `h-80`, `h-96`, and 45%-width sections. If legal values overflow, revisit the field-specific limits with the inventory approval rather than introducing a general redesign.

## Acceptance criteria and validation

- [x] Every approved About slot reads from `about`; all exclusions remain unchanged.
- [x] Empty database and failed public loads preserve the original readable wording.
- [x] An Admin can save multiple fields together and see publication immediately; ordinary users see no edit action.
- [x] Numeric facts, images, contact form, styling, element types, and section composition are unchanged, apart from the inventory-approved text containment safeguards below.
- [x] Email/phone changes affect display only; multiline/default/max-length text is visually usable.

Add integration/component tests for defaults, overrides, literal markup display, Admin gating, and excluded values. Run focused Vitest tests and frontend build. Manually check wide/narrow layouts and existing contact form behavior with no unrelated layout fixes.

## Implementation and handoff

`AboutComponent.vue` loads `about` once through the shared store, binds all 15
approved fields reactively, and mounts `SiteTextEditAction` without an enclosing
Admin-only condition. Public load failures leave defaults readable; the shared
editor still requires its own successful refresh. All values use interpolation.
Only the introduction supports multiline presentation (`whitespace-pre-line`).

`+550`, `5`, `1903`, `Tlf. `, and `Email: ` remain template-owned. Images,
heading/paragraph element types, section order, 45%-width columns, and the nested
contact form are preserved. No registry, endpoint, mail configuration, form
destination, or persistence changes are needed.

The [approved inventory](field-inventory.md#layout-review-and-follow-up)
permits narrow containment safeguards: the introduction's `h-80` and secretariat's
`h-96` become `min-h-80` and `min-h-96`, columns can shrink with `min-w-0`, and
component-scoped heading/prose wrapping contains unbroken legal values. The section
image remains `h-80`. No stacking, truncation, shared-shell fixes, field-limit
changes, or general redesign was introduced. Very newline-heavy prose grows the
section naturally rather than overlapping the next section.

## Validation record (2026-11-25)

Reused the clean, correctly scoped `feat/text-modification` branch after fetching
and confirming GitHub's current default branch (`master`) remains its ancestor.
Validation uses Bun 1.4.4:

| Check | Result |
| --- | --- |
| Focused About Vitest tests | Passed: seven component/integration tests |
| `bun vitest run` | Passed: 121 tests across seven files |
| `bun run build` | Passed: Vue/TypeScript checks and Vite production build |
| `bun lint` | Passed; no unrelated changes |
| Headless Chromium (`playwright-cli`) | Passed on the real `/about` route at 1440×900, 390×900, and 320×900 |
| `git diff --check` / diff review | Passed |

Tests cover all independent bindings (including the two distinct section labels),
empty overrides and failed loads, literal HTML-looking values, preserved multiline
prose, reactive Admin gating and draft retention on logout, immediate publication
of a two-field save, excluded facts/images/elements, and unchanged contact payloads.
The existing editor tests additionally cover failed saves and all dismissal paths.

Browser checks exercise defaults, maximum-length spaced and unbroken values for
every field (including a valid 80-character email), and a 450-character multiline
introduction at all three widths. Editable text has no measured horizontal overflow;
the paragraph and portrait remain contained before subsequent sections. Wide/narrow
rendering was inspected, including a full-page mobile screenshot. The original
two-column presentation remains cramped on narrow screens. The excluded shared
shell/footer still produces document widths of 485px at 390px and 433px at 320px,
matching the inventory baseline; it was not changed.

Browser checks also verify visitor action hiding, literal markup, public failure
fallback, draft isolation/cancellation, one two-field PATCH updating the page
immediately, and persisted wording on reload. After a reload, the unchanged contact
form posts the sender's details to `/information`, clears on success, and does not
use the edited display email as a destination.

Vite ran only on `127.0.0.1:5187` with non-mock `AboutCheck` mode and an explicitly
loopback API base (`127.0.0.1:5186`). Playwright intercepted API responses; no actual
API/database integration or email delivery was exercised. The deliberate GET 500
and the initial pre-interception connection refusal explain browser network errors;
no application runtime errors were observed. The dedicated browser/server were
stopped. The running Docker stack was left untouched: no API startup, migrations,
database writes, or infrastructure actions were needed.

Existing tooling deprecation/experimental-localStorage warnings remain. Whole-feature
cross-layer validation remains in 10. Commits are local only; no push or PR was
performed, as requested. Implementation approval remains a human decision.
