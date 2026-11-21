# 07 — Integrate editable About text

**Status:** Not started

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

- [ ] Every approved About slot reads from `about`; all exclusions remain unchanged.
- [ ] Empty database and failed public loads preserve the original readable wording.
- [ ] An Admin can save multiple fields together and see publication immediately; ordinary users see no edit action.
- [ ] Numeric facts, images, contact form, styling, element types, and section composition are unchanged.
- [ ] Email/phone changes affect display only; multiline/default/max-length text is visually usable.

Add integration/component tests for defaults, overrides, literal markup display, Admin gating, and excluded values. Run focused Vitest tests and frontend build. Manually check wide/narrow layouts and existing contact form behavior with no unrelated layout fixes.
