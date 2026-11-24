# Site-text editing implementation work items

These work items turn the [design plan](../site-text-editing.md) into an ordered implementation plan. The design remains authoritative. The numbered items distinguish implemented foundations from later application behavior; the inventory's approval record is authoritative for editorial choices.

## Execution order

| ID | Work item | Depends on | Deliverable |
| --- | --- | --- | --- |
| 01 | [Approve the editable-field inventory](01-field-inventory.md) | None | Approved slots, defaults, grouping, exclusions, and layout limits |
| 02 | [Define the registry and API contract](02-definitions-and-contract.md) | 01 | Matching field definitions, typed contracts, and registry checks |
| 03 | [Add override persistence](03-persistence.md) | 02 | Separate `SiteTexts` table, migration, and atomic repository operations |
| 04 | [Implement the generic API](04-api.md) | 02, 03 | Public group reads and validated, admin-only batch updates |
| 05 | [Build shared frontend state](05-frontend-state.md) | 02, 04 | Group loading, fallback defaults, PATCH client, and reactive publication |
| 06 | [Build the reusable editor](06-editor.md) | 05 | Danish, sectioned modal with isolated edits and failure handling |
| 07 | [Integrate About](07-about.md) | 06 | Editable approved About slots |
| 08 | [Integrate Home](08-home.md) | 06 | Editable Home slots and shared membership benefits |
| 09 | [Integrate Member Advantages](09-member-advantages.md) | 06 | Editable page slots and the same shared membership benefits |
| 10 | [Verify and document the delivery](10-verification-and-handoff.md) | 07, 08, 09 | Cross-layer verification, extension guide, and human handoff |

Follow the numbered order by default. After 06, the three component integrations have no implementation dependency on each other; their shared-content behavior is verified together in 10. Add focused tests with each item, rather than postponing all testing until 10. Keep each item's completion checklist unchecked until its implementation and validation are complete.

## Proposed implementation baseline

- Retain Vue templates, existing authentication, `BaseModal.vue`, the API client, and PostgreSQL. No CMS, new content service, or additional EF context.
- Add a standalone entity/table to `ContentsContext`; do not inherit from `BaseContent` or expose site text in the event/news feed.
- Maintain modestly duplicated JSON definitions within the frontend and API packages, with an automated parity check. Both Docker builds currently use package-local contexts; a repository-level shared runtime file would otherwise require build-context changes. Do not introduce code generation.
- Use `GET /SiteTexts/{group}` and `PATCH /SiteTexts/{group}` with changed key/value pairs. Return overrides, not merged defaults or audit identities.
- Save one group atomically. A page using both its own group and `membership` has separate, clearly labelled editing actions; do not imply that two requests form one atomic page save.
- Use last-committed-write-wins for overlapping submitted keys. Omitted keys remain untouched. Do not add revisions, ETags, optimistic-locking UI, or cross-browser synchronization initially.
- Exclude restore-default controls from the initial delivery. Empty optional text remains a real override, not a reset operation.

Item 01 is an explicit human approval gate for the inventory and these baseline choices. Document agreed changes before implementing 02; do not silently treat proposals as decisions already made by the user.

### Current checkpoint

Item 01's [73-field inventory](field-inventory.md#approval-record) and all baseline
decisions were approved on 2026-11-24. Item 02 adds matching package-local
registries, immutable accessors, typed wire contracts, and executable value and
envelope rules. Its implementation at `e7a6f33` was
[human approved on 2026-11-24](02-definitions-and-contract.md#human-approval-record).
See its [handoff](02-definitions-and-contract.md#implementation-and-handoff)
for the contract and validation record. Item 03 now implements the separate
override table/migration and transactional PostgreSQL repository, with opt-in
database tests. Its implementation at `57f2091` was
[human approved on 2026-11-24](03-persistence.md#human-approval-record). See its
[handoff](03-persistence.md#implementation-and-handoff-to-04) for repository usage,
validation, and migration/rollback requirements. HTTP endpoints, frontend
state/editor, and component bindings remain in 04–09; no editable UI is available yet.

## Scope boundaries and operating rules

Only deliberately approved plain-text slots in About, Home, and Member Advantages are in scope. Images, layout, list membership/order, routing, business fees, data-derived statistics, nested component forms, drafts, scheduling, and revision history remain unchanged.

Use [contributing.md](../../contributing.md), the [frontend guide](../../src/dkef-vue/AGENTS.md), and the [API guide](../../src/dkef-api/AGENTS.md) throughout implementation. Confirm disposable local database targets before applying migrations or starting the API: startup migrates all six contexts. Do not execute Ansible playbooks or deploy.

Item 03's human approval is recorded; await further instructions before beginning
04. Keep commits local: do not push or open a PR, as requested.
The later work items still require their own validation and human review.
