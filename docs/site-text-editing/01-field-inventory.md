# 01 — Approve the editable-field inventory

**Status:** Inventory prepared and validated; approved

**Depends on:** None

**Blocks:** [02](02-definitions-and-contract.md)

## Goal

Agree exactly which slots administrators may edit before building persistence or forms. Produce a reviewed inventory in this directory; do not expand scope merely because a string appears in a selected component.

## Review deliverable

The [explicit field inventory](field-inventory.md) expands all proposed slots into
73 named fields, including exact defaults, Danish editor/section labels, required
status, input kinds, numerical limits, format rules, order, and 78 source bindings.
It also records proposed exclusions and baseline decisions, source/default checks,
and browser review findings at desktop and narrow widths.

**Human approval is recorded in the inventory.** In particular, About already overflows on
narrow screens with its default paragraph, valid long email values overflow, and
maximum-length unbroken text exposes wrapping limitations on all three pages.
The inventory records accepted follow-up boundaries for 07–09; no layout fixes or later
feature work are implemented here. The inventory's
[approval record](field-inventory.md#approval-record) accepts all decisions on 2026-11-24.

## Proposed inventory to review

All identifiers below are proposals. A prefix denotes separate named fields, not an administrator-managed list. Copy the exact current visible wording into each final definition, accounting for HTML whitespace collapsing rather than copying template indentation into paragraphs.

| Group | Semantic slots/prefixes | Existing content and editorial intent |
| --- | --- | --- |
| `about` | `page.title`, `page.subtitle` | Main heading and association tagline |
| `about` | `introduction.title`, `introduction.description`, `sections.title` | Association introduction and section-image heading |
| `about` | `statistics.members.label`, `statistics.sections.label`, `statistics.founded.label` | Labels only; numeric values remain code-owned |
| `about` | `secretariat.title`, `secretariat.name`, `secretariat.address`, `secretariat.postal-city`, `secretariat.phone`, `secretariat.email`, `contact.title` | Display-only contact information and the heading above the existing contact form |
| `home` | `hero.eyebrow`, `hero.title`, `hero.description`, `companies.title` | Hero wording and member-company strip heading |
| `home` | `network.eyebrow`, `network.title`, `network.description`, `network.action-label` | Network section; description currently includes the static claim of over 550 members |
| `home` | `development.eyebrow`, `development.title`, `development.description`, `development.action-label` | Professional-development section |
| `home` | `reasons.eyebrow`, `reasons.title`, `reasons.description`, `reasons.action-label` | Introduction to the three developer-owned reasons |
| `home` | `reasons.sparring.title/description`, `reasons.network.title/description`, `reasons.development.title/description` | Text in three fixed cards; slash notation means two separate keys |
| `home` | `plans.title`, `plan.title`, `plan.description`, `plan.activities-description`, `plan.action-label` | Membership-card headings, paragraphs, and CTA label, not its price |
| `membership` | `benefits.development`, `benefits.network`, `benefits.mentoring`, `benefits.events`, `benefits.magazine` | The same five benefits in Home and Member Advantages; checkmarks stay in templates |
| `member-advantages` | `page.title`, `introduction.eyebrow`, `introduction.title`, `introduction.description`, `introduction.action-label`, `membership.title` | Page title, opening promotion, CTA label, and membership-list heading |
| `member-advantages` | `student-offers.title`, `student-offers.mentoring`, `student-offers.network-meetings`, `student-offers.project-network`, `student-offers.jobs`, `student-offers.included-benefits` | Heading and five fixed student-offer entries; bullet characters stay in templates |
| `member-advantages` | `network.eyebrow/title/description`, `events.eyebrow/title/description` | Separate network and events promotional sections |
| `member-advantages` | `other-benefits.eyebrow`, `other-benefits.title`, `mentoring.title/description`, `sparring.title/description`, `magazine.title/description` | Introduction and three fixed benefit cards |

Do not share Home's three reason cards with longer Member Advantages copy just because both discuss networking or development. Student-offer mentoring also has a distinct purpose from the shared membership-list item. Repeated CTA labels remain page/section-specific slots unless sharing is explicitly approved.

## Tasks

1. Inspect `src/dkef-vue/src/components/AboutComponent.vue`, `HomeComponent.vue`, and `MemberAdvantages.vue` alongside the rendered pages. Expand every proposed prefix into explicit group/key rows and source bindings.
2. Record, per field: exact default, Danish editor label, Danish section label, input kind, required flag, numerical maximum length, and any format rule. Define deterministic developer-owned field/section order.
3. Use layout-informed starting limits: button labels 32 characters, eyebrow/list labels 60, headings 100, short descriptions 300, and longer paragraphs 800. These are review candidates, not universal limits: ensure every default fits, and tighten/adjust for the actual slot. Pay particular attention to About's `h-80` introduction, `h-96` secretariat, and 45%-width columns. Test narrow viewports and long words; character counts alone cannot guarantee fit.
4. Propose required values for primary headings, CTA labels, and fixed list entries. Allow optional empty values only where an empty slot is explicitly acceptable. Required whitespace-only values must fail. Do not make all fields optional by default.
5. For contact email, propose a bare email value with code-owned `Email: ` prefix and email validation. For phone, decide whether `Tlf. ` is code-owned. Preserve the current visible output. These display values must not change contact-form destinations, login identities, or backend mail configuration.
6. Record explicit exclusions: About's `+550`, `5`, and `1903`; Home's review/rating claim, `700,-`, and price/tax/year suffix; images/alt text, `MigrationBanner`, disabled newsletter, and internals of `ContactFormularComponent`. Confirm whether the static member-count claim inside Home's network paragraph may remain editable as editorial copy; do not add an independent editable numeric statistic or a data binding as incidental work.
7. Review the baseline choices in [the index](README.md), including deliberate benefit sharing, one-group saves, last-write-wins, and no restore-default UI. Obtain human approval and record accepted inventory/decisions and unresolved exclusions before 02 begins.

## Acceptance criteria

- [x] Every selected slot has an explicit semantic key, full default, Danish labels, validation, and source location.
- [x] Each fixed list entry is individually named; no identifiers depend on array positions or Vue filenames.
- [x] Shared membership wording and page-specific wording are explicitly distinguished.
- [x] Prices, statistics, contact side effects, and nested components have recorded scope decisions.
- [x] Maximum-length/default wording examples have been visually reviewed in the affected layouts.
- [x] A human has approved the inventory and implementation baseline; proposals are not mistaken for approval.

## Validation and handoff

Review every inventory row against the source and rendered default text. Check that all defaults satisfy their proposed rules. This item is complete only with the recorded approval; pass the approved inventory to 02, 07, 08, and 09.
