# Administrator site-text editing

Administrators can update selected website text without changing code or deploying the application. Saved changes are published immediately for all visitors.

## Functionality

- **About:** edit headings, the association description, statistics labels, secretariat contact details, and the board heading.
- **Home:** edit the hero, network and benefits sections, membership call to action, and upcoming-events heading.
- **Member Advantages:** edit page headings, benefit-card text, membership-price display text, and call-to-action labels.
- **Shared membership benefits:** edit the five benefit descriptions from either Home or Member Advantages. These values appear on both pages; other page text is saved separately.

When logged in with the `Admin` role, use the relevant **Rediger tekster** action. The editor fetches the current values and presents Danish-labelled fields grouped into sections, using inputs for short text and textareas for multiline content. Required fields, character limits, and email/phone rules are shown and validated.

**Gem ændringer** saves only changed fields and closes the editor after success. All changes in that save succeed or fail together. **Annuller** discards unsaved edits. A failed save retains the draft and displays a Danish error; a failed initial load must be retried before editing. Losing administrator access prevents saving without discarding an open draft.

Components retain control over layout, images, links, and the number/order of sections and list entries. Text is rendered as plain text, not HTML. Editing displayed prices does not change membership fees or other business rules; statistics values and dynamic event data remain outside the editor. There are no drafts, approval workflows, scheduling, revision history, or reset-to-default action.

Main page images have a separate [administrator image editor](site-image-editing.md); they are not stored or edited as site text.

## Technical implementation

### Definitions and frontend

Matching `siteTextDefinitions.json` registries live in [frontend content](../src/dkef-vue/src/content/siteTextDefinitions.json) and [API configuration](../src/dkef-api/Configuration/siteTextDefinitions.json). They define stable group/key identifiers, default wording, Danish labels, section/field order, input types, and validation rules. The four groups are `about`, `home`, `membership`, and `member-advantages` (73 fields in total). Tests check registry parity and value-rule consistency across C# and TypeScript.

The Pinia [site-text store](../src/dkef-vue/src/stores/siteTextStore.ts) merges saved overrides with bundled defaults and reuses loaded groups in memory, without persistent browser storage. Missing overrides use defaults; failed public reads leave defaults or previously loaded text visible. Opening the editor always refreshes the group. A successful save updates all mounted consumers of that group; other browsers see changes on a fresh load, not through live synchronization.

[SiteTextEditAction.vue](../src/dkef-vue/src/components/SiteTextEditAction.vue) exposes admin-only actions, while [SiteTextEditorModal.vue](../src/dkef-vue/src/components/SiteTextEditorModal.vue) builds the reusable form on `BaseModal.vue` and keeps a detached draft. Requests use the existing API client and URL service.

### API and persistence

[SiteTextsController](../src/dkef-api/Controllers/SiteTextsController.cs) provides:

| Endpoint | Access | Behavior |
| --- | --- | --- |
| `GET /SiteTexts/{group}` | Public | Returns registered saved overrides with `Cache-Control: no-store`. |
| `PATCH /SiteTexts/{group}` | `Admin` role | Validates and atomically saves the submitted changes, then returns the full group override snapshot after commit. |

PATCH accepts a non-empty `changes` object, for example:

```json
{"changes":{"hero.title":"Dit netværk i elbranchen"}}
```

Both successful endpoints return the same shape:

```json
{"group":"home","overrides":{"hero.title":"Dit netværk i elbranchen"}}
```

Group/key identifiers are case-sensitive. Unknown groups return 404; anonymous writes return 401 and authenticated non-admin writes return 403. The API bounds request size, rejects malformed/duplicate/unknown fields, sanitizes submitted text, and enforces field validation before writing. Validation errors use Danish problem details, including field-specific errors; defaults and audit metadata are not returned.

The [repository](../src/dkef-api/Repositories/SiteTextRepository.cs) stores overrides in the PostgreSQL `SiteTexts` table through `ContentsContext`, keyed by `(Group, Key)` with `Value`, `UpdatedAt`, and the authenticated updater's Identity ID in `UpdatedBy`. Transactional upserts leave omitted keys unchanged. Concurrent edits to different keys survive; overlapping edits use last-committed-write-wins, without conflict detection. Saved overrides continue to take precedence over later default changes; an optional empty value is distinct from a missing override.

The `AddSiteTexts` migration creates the table without seeding overrides. Deployment and migrations remain human-operated; API startup applies migrations automatically, so verify database targets first. Rolling back this migration deletes all saved site-text overrides.

To add editable text, update both registries and bind the component to the shared store/editor. No new table, endpoint, or bespoke form is needed. See [backend validation](../contributing.md#backend-validation) for database-free and opt-in integration tests.
