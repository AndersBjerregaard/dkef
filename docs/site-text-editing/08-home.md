# 08 — Integrate editable Home text

**Status:** Implemented; awaiting human review

**Depends on:** [06](06-editor.md) and the approved inventory in [01](01-field-inventory.md)

**Blocks:** [10](10-verification-and-handoff.md)

## Goal and files

Connect `src/dkef-vue/src/components/HomeComponent.vue` to `home` and the intentionally shared `membership` group while preserving routing, images, and the existing promotional layout.

## Tasks

1. Bind the approved hero, company heading, network/development sections, three-reasons introduction/cards, and membership-card prose/labels to `home` values.
2. Bind the five fixed plan benefits to named `membership` keys. Retain checkmarks, list membership/order, and element types in the template. Do not merge the three reason cards into the shared list or bind them by numeric indices.
3. Add separate Admin-only shared editing actions: `Rediger forsidens tekster` for `home` and `Rediger fælles medlemsfordele` for `membership`. Explain in Danish that shared benefit edits also appear on the Member Advantages page. Each modal saves one group atomically; do not present a two-request Save All.
4. Keep `memberAdvantages()`/`register()` handlers, route names, scroll behavior, and button destinations unchanged when labels become editable.
5. Leave the price `700,-`, tax/year suffix, rating/review claim, images/logos/alt text, `MigrationBanner`, and disabled newsletter outside the feature as agreed. Handle the member-count claim inside network prose according to 01's explicit decision; do not create a new business/statistics model.
6. Preserve original wording and component-owned multiline presentation. Review narrow widths, 96-width content columns, `h-12` buttons, and the 80-width membership card at approved maximum lengths.

## Acceptance criteria and validation

- [x] All approved Home slots are bound, with initial wording unchanged when no overrides exist.
- [x] The five shared benefits use exactly the registry keys approved for 09; sharing is not inferred from string equality. The second consumer remains for 09, with joint verification in 10.
- [x] Separate editor actions communicate group boundaries and the effect of shared edits in Danish.
- [x] Saved values display immediately; failed loads/saves and cancelled drafts do not remove or publish content.
- [x] Price/claims exclusions, images, card count/order, migration banner, navigation, and layout remain developer-controlled.

Add component tests for both groups, defaults/fallback, Admin gating, fixed list order, excluded price/rating, and unchanged CTA navigation. Run focused Vitest tests and frontend build. Verify maximum-length and multiline values in mobile/desktop layouts; shared-consumer publication is completed in 10 after both integrations exist.

## Implementation and handoff

`HomeComponent.vue` loads `home` and `membership` independently through the shared
store and binds all 27 Home fields and five named shared benefits. A failed group
load does not block the other group or remove defaults. Each benefit remains an
explicit paragraph with its template-owned checkmark and fixed order. The three
reason cards retain their own Home keys, fixed order, and element types.

Both `SiteTextEditAction` instances remain mounted, with Admin-only buttons labelled
`Rediger forsidens tekster` and `Rediger fælles medlemsfordele`. The shared action
explains inside its modal, in Danish, that these five values also appear on the
Medlemsfordele page and that other Home text is saved separately. The reusable
action accepts optional `label` and `description` props and forwards the latter to
the existing modal. Other consumers retain the original default action label.
Each editor refreshes and saves exactly one group; there is no two-group Save All.

All fields use plain Vue interpolation. All nine approved multiline descriptions
use component-owned `whitespace-pre-line`. The complete network description,
including the static member-count claim, is editorial copy as approved in 01;
there is no new statistic model or synchronization with About. Price `700,-`, its
tax/year suffix, the rating/review claim, all images/logos/alt text, MigrationBanner,
and the commented-out newsletter remain unchanged. Both navigation handlers,
route names, and smooth scrolling are unchanged.

The [approved containment follow-up](field-inventory.md#layout-review-and-follow-up)
is limited to Home: shrinkable containers, safe wrapping for headings/prose/buttons,
`max-w-full` on the existing `w-80` membership card, and buttons that retain a 3rem
minimum height but grow when long labels wrap. Existing colors, spacing, 96-width
columns, card/list structure, and destinations remain component-owned. Text is
neither truncated nor clipped; newline-heavy prose grows sections naturally.

## Validation record (2026-11-25)

Reused the clean, correctly scoped `feat/text-modification` branch after fetching
and confirming GitHub's default branch (`master`) remains its ancestor. Checks use
Bun 1.4.4 and the installed dependencies:

| Check | Result |
| --- | --- |
| Home component/integration tests | Passed: 12 tests, including both group variants |
| `bun vitest run` | Passed: 133 tests across eight files, including About and shared-editor regressions |
| `bun run build` | Passed: Vue/TypeScript checks and Vite production build |
| `bun lint` | Passed; no unrelated changes |
| Headless Chromium (`playwright-cli`) | Passed on `/` at 1440×900, 390×900, and 320×900 |
| `git diff --check` / diff review | Passed |

Component tests cover every independent slot in exact element/order, original
defaults before and after empty/failed loads, independent group failures, all
multiline slots, literal markup display, reactive Admin gating, role loss, separate
multi-field one-group PATCHes, immediate publication, cancelled drafts, failed-save
draft retention, failed-editor-load blocking, unchanged media/facts/banner, and all
four CTA route/scroll behaviors with changed labels.

Browser checks exercise both editor save flows and the Danish sharing explanation
using intercepted API responses. The layout matrix has 12 cases: defaults and
simultaneous field-maximum spaced, unbroken `W`, and newline-heavy values at each
width. Newline-heavy probes use repeated `W\n` up to each multiline field's limit;
single-line slots remain at their legal unbroken maximum. Geometry checks found no
editable-slot horizontal overflow or text/button clipping. Full-page default and
unbroken screenshots at desktop/mobile were visually inspected. The 320px
membership section now stays inside its content bounds. The excluded shared
shell/footer retains document widths of 485px at 390px and 433px at 320px, matching
01's baseline rather than introducing a new editable-text regression.

Vite used non-development `Local` mode with an explicit loopback-only API base
(`http://127.0.0.1:5173/api`). All API requests in the final browser run were
intercepted; there were no browser console errors/warnings. No backend startup,
migrations, real authentication, database writes, or infrastructure actions were
needed. The existing Docker stack was left untouched. Browser artifacts remain
outside commits; the dedicated browser and Vite server were stopped after checks.

Existing tooling deprecation/experimental-localStorage warnings remain. Real
API/database integration and publication across both Home and Member Advantages
remain in 10 after 09 connects the second consumer. Commits are local only; no push
or PR was performed, as requested. Implementation approval remains a human decision.
