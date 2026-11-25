# 09 — Integrate editable Member Advantages text

**Status:** Implemented; awaiting human review

**Depends on:** [06](06-editor.md) and the approved inventory in [01](01-field-inventory.md)

**Blocks:** [10](10-verification-and-handoff.md)

## Goal and files

Connect `src/dkef-vue/src/components/MemberAdvantages.vue` to `member-advantages` plus the same `membership` group used by Home, without adding new sections or editor forms.

## Tasks

1. Bind approved page/introduction headings, prose and CTA label; membership/student-offer headings; five fixed student offers; network/events sections; and the three remaining benefit cards to `member-advantages`.
2. Bind the five shared membership-list entries to `membership` using the exact semantic keys from 02/08. Keep the student-offer mentoring item and longer card descriptions page-specific even where concepts overlap.
3. Add separate Admin-only shared actions: `Rediger sidens tekster` and `Rediger fælles medlemsfordele`. Explain that the latter also updates Home. Keep local drafts/group saves isolated using 06.
4. Preserve checkmarks, bullets, five student entries, card order/count, three card images, network/events images, divider structure, responsive classes, and the `goto('register')` navigation/scroll behavior.
5. Render only interpolated text. Apply line-break preservation only to approved multiline slots. Keep limits and default wrapping usable in 72-width cards and 96-width text columns without a page redesign.

## Acceptance criteria and validation

- [x] Every approved page-specific slot uses `member-advantages`; shared benefits use `membership` and not duplicate page-local overrides.
- [x] Default wording, list order, page structure, images, and register navigation remain unchanged.
- [x] An Admin can publish page text and shared benefit text through the reusable editor; non-admins cannot access the actions.
- [x] A successful membership save updates mounted Home and Member Advantages consumers; editing a page-specific field does not change the other page.
- [x] Public-load fallback, failed saves, cancellation, and maximum-length/multiline rendering behave as designed.

Add component tests for both groups, deliberate/non-sharing, fixed lists/cards, Admin gating, and unchanged CTA behavior. Run focused Vitest tests and frontend build. Test both components mounted against one store to verify immediate shared publication without relying on route reloads; repeat the full API-backed flow in 10.

## Implementation and handoff

`MemberAdvantages.vue` loads `member-advantages` and `membership` independently
through the existing store. All 26 page-specific fields use the former; the five
fixed membership entries use exactly Home's `benefits.development`,
`benefits.network`, `benefits.mentoring`, `benefits.events`, and `benefits.magazine`
keys. Student-offer mentoring and the longer benefit-card descriptions remain
page-specific. Defaults and failed public loads remain readable.

The existing reusable actions provide `Rediger sidens tekster` and
`Rediger fælles medlemsfordele`. The latter explains in Danish that publication
also affects Home and that the page's other text is saved separately. Each action
opens its own one-group editor; the actions stay mounted so role loss cannot
destroy an open draft. No editor, registry, API, or schema changes were needed.

All fields render through plain interpolation. Only the six approved multiline
descriptions preserve line breaks. Existing headings, fixed lists and card order,
checkmarks, bullets, five images/alt text, dividers, responsive classes, and
`goto('register')`/smooth scrolling are retained. The approved containment follow-up
is limited to shrinkable text containers, safe wrapping, non-shrinking student
bullets with separately wrapping text spans, and buttons that grow beyond their
existing 3rem minimum height. Text is not truncated or clipped, and no sections,
cards, or administrator-managed list structure were added.

## Validation record (2026-11-25)

Reused the clean, correctly scoped `feat/text-modification` branch after fetching
and confirming GitHub's default branch (`master`) remains its ancestor. Checks
used Bun 1.4.4 and installed dependencies:

| Check | Result |
| --- | --- |
| Member Advantages component/integration tests | Passed: 13 tests, including parameterized group and publisher variants |
| `bun vitest run` | Passed: 146 tests across nine files |
| `bun run build` | Passed: Vue/TypeScript checks and Vite production build |
| `bun lint` | Passed; no unrelated changes |
| Headless Chromium (`playwright-cli`) | Passed on `/advantages` at 1440×900, 390×900, and 320×900 |
| `git diff --check` / diff review | Passed |

Component tests cover every independent slot in fixed element/order, readable
defaults before and after empty/failed loads, independent group failures, all six
multiline slots, literal markup display, reactive Admin gating and role loss,
cancelled drafts, retained failed-save drafts, blocked failed-editor-load saves,
all field limits/oversized input, unchanged images/dividers/list and card counts,
and the unchanged CTA destination/scroll behavior with an edited label.

Joint integration tests mount real Home and Member Advantages components against
one Pinia store. They verify a single initial membership GET, multi-field one-group
PATCH publication from either component immediately updating both mounted pages,
and page-specific student mentoring/card saves changing neither Home nor shared
membership values. The real reusable editor/modal is used, not a bespoke test
editor, and no route reload is involved.

Browser checks use intercepted API responses, not a real backend. Both editor
save flows and the Danish sharing explanation passed. The layout matrix covers
defaults and simultaneous maximum-length spaced, unbroken `W`, and newline-heavy
values at all three widths (12 cases). Multiline probes repeat `W\n` up to each
field's limit; single-line fields use legal unbroken maxima. Geometry checks found
no editable-slot horizontal overflow or text/button clipping. Default and unbroken
desktop/mobile screenshots were captured; the default mobile and unbroken desktop
screenshots were visually reviewed. The shared shell/footer's pre-existing narrow
screen overflow remains outside this integration's scope, as recorded in 01/08.

Vite used non-development `Local` behavior with mode `membercheck` and the explicit
loopback-only API base `http://127.0.0.1:5189/api`; all browser API requests were
intercepted. Final console checks reported no errors or warnings. The dedicated
browser and Vite server were stopped, and generated artifacts are not committed.
No backend startup, migrations, real authentication, database writes, or Docker
changes were needed. The user's running local stack was left untouched.

Existing tooling deprecation/experimental-localStorage warnings remain. Full real
API/database integration, persistence after reload, and whole-feature verification
remain in 10. Commits are local only; no push or PR was performed, as requested.
Implementation approval remains a human decision.
