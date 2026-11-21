# 10 — Verify the complete feature and document extension/handoff

**Status:** Not started

**Depends on:** [07](07-about.md), [08](08-home.md), [09](09-member-advantages.md)

## Goal

Demonstrate all design success criteria across API, database, shared state, editor, and initial pages; document how to extend the feature and how a human safely rolls it out.

## Verification matrix

| Scenario | Required result |
| --- | --- |
| Registered group without rows | Public API returns a successful empty override set; all three pages display original defaults |
| First public request fails | Pages remain readable; editor separately reports failed load and cannot save |
| Editor refresh fails after a cached success | Existing display remains readable, drafts are not fabricated from defaults, Save stays blocked until retry succeeds |
| Anonymous/member/Board Member PATCH | `401`/`403` as appropriate, with no rows or audit metadata changed |
| Admin edits multiple fields | One atomic group save; saved wording is immediately visible locally and visible to a new visitor load |
| One invalid field in a valid batch | Entire request rejected; no partial publication |
| Database/network/auth save failure | Published state unchanged; all unsaved working values retained with useful Danish feedback |
| Cancel/close/Escape/backdrop | Unsaved values discarded, published values unchanged, fresh draft on reopen |
| Optional empty versus missing | Stored `""` remains empty; missing row falls back to default; required whitespace-only text rejected |
| Markup, entities, Danish letters, multiline text | No executable markup or `v-html`; agreed sanitization, escaping, and line breaks round-trip without double encoding |
| Minimum/maximum/over-limit values | Rules match frontend/backend; accepted values fit the existing layout; invalid values do not save |
| Simultaneous edits | Same submitted key follows last-committed-write-wins, omitted/disjoint keys survive, concurrent first insert succeeds atomically |
| Late GET after PATCH | A stale response cannot overwrite the newer published group state |
| Shared membership save | Both mounted components update immediately; a fresh browser load sees the stored override |
| Default changed in a later build | Existing override still wins; fields without overrides use the updated default |
| New registered slot/group | Definitions and component binding suffice; no new table, controller, endpoint, or bespoke form |
| Existing site behavior | Images, factual price/statistic exclusions, contact delivery, navigation, section/card/list order, and layout remain intact |

## Tasks and checks

1. Run the focused tests added in 02–09, then `bun vitest run`, `bun lint`, and `bun run build` from `src/dkef-vue`. Review auto-fixes for unrelated churn. Run `dotnet restore`, `dotnet build`, and the explicit new test-project command if added. Record passed/failed/blocked checks rather than implying release-image CI provides test coverage.
2. Confirm disposable local targets for all six API contexts before startup/database tests. Run the matrix against the actual local PostgreSQL/API and a non-mock frontend mode. Do not use shared data for authorization, concurrency, migration, or failure-injection checks. Never commit test tokens, credentials, or local configuration.
3. Review defaults and longest valid edits on narrow/wide screens and light/dark themes. Check keyboard-only modal operation, focus, Danish loading/error/success messages, and all close paths. Resolve layout-limit discrepancies without unrelated redesign.
4. Verify migration apply/rollback and package-local registry inclusion in API publish/frontend build. If local Docker image builds are needed, confirm the Docker context first and use existing package contexts; do not publish images or invoke release workflows as a validation shortcut.
5. Add a developer extension guide in this directory or another clearly linked document: choose stable semantic group/keys, update both registries/parity tests, define layout-appropriate constraints/Danish labels, bind interpolated values, and reuse the group editor. Explain deliberate sharing, optional empty values, override precedence, no reset UI, and explicit override preservation/migration decisions for future key renames/removals. Do not delete retired rows implicitly.
6. Add an administrator guide explaining group-specific immediate publication, shared membership effects, cancellation, load/save errors, and last-write-wins limitations. Clarify that editing displayed text cannot change prices/business rules, images, navigation, or section structure.
7. Document the human rollout order: back up the target database, review/apply the API migration through the normal approved deployment process, make the new API available before the frontend, then smoke-test public reads and authorized saves. API startup already applies migrations; operators must account for that. Older API/frontend compatibility, rollback consequences, and preserving override data must be explicit. Do not run Ansible playbooks or perform deployment.
8. Review the feature diff and run `git diff --check`. Mark work items complete only with evidence and record any deviations/remaining limitations. At full feature completion, follow `contributing.md` for a feature-branch push and PR against the GitHub-discovered default branch, using any repository PR template. Leave it for human review; do not merge or deploy. This is a later handoff, not authorization to push at the current documentation-only checkpoint.

## Acceptance criteria

- [ ] Every matrix scenario has automated or recorded repeatable local/manual evidence.
- [ ] Server permissions, atomicity, plain-text rules, defaults, and draft isolation satisfy the design.
- [ ] All three initial components work without changes to their developer-owned structure or business behavior.
- [ ] Extending content requires only registry entries and shared mechanism bindings, with registry parity maintained.
- [ ] Operator/admin/developer documentation covers the supported behavior and explicit limitations.
- [ ] Validation results and migration risks are ready for human review; no shared database, infrastructure, or deployment has been modified by the agent.
