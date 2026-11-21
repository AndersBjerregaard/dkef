# 08 — Integrate editable Home text

**Status:** Not started

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

- [ ] All approved Home slots are bound, with initial wording unchanged when no overrides exist.
- [ ] The five shared benefits use exactly the registry keys that 09 uses; sharing is not inferred from string equality.
- [ ] Separate editor actions communicate group boundaries and the effect of shared edits in Danish.
- [ ] Saved values display immediately; failed loads/saves and cancelled drafts do not remove or publish content.
- [ ] Price/claims exclusions, images, card count/order, migration banner, navigation, and layout remain developer-controlled.

Add component tests for both groups, defaults/fallback, Admin gating, fixed list order, excluded price/rating, and unchanged CTA navigation. Run focused Vitest tests and frontend build. Verify maximum-length and multiline values in mobile/desktop layouts; shared-consumer publication is completed in 10 after both integrations exist.
