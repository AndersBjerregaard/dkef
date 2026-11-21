# 09 — Integrate editable Member Advantages text

**Status:** Not started

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

- [ ] Every approved page-specific slot uses `member-advantages`; shared benefits use `membership` and not duplicate page-local overrides.
- [ ] Default wording, list order, page structure, images, and register navigation remain unchanged.
- [ ] An Admin can publish page text and shared benefit text through the reusable editor; non-admins cannot access the actions.
- [ ] A successful membership save updates mounted Home and Member Advantages consumers; editing a page-specific field does not change the other page.
- [ ] Public-load fallback, failed saves, cancellation, and maximum-length/multiline rendering behave as designed.

Add component tests for both groups, deliberate/non-sharing, fixed lists/cards, Admin gating, and unchanged CTA behavior. Run focused Vitest tests and frontend build. Test both components mounted against one store to verify immediate shared publication without relying on route reloads; repeat the full API-backed flow in 10.
