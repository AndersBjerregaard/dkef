# Administrator-editable site text

## Purpose

Allow administrators to update selected text on the website without requiring a code change or deployment. The initial scope covers:

- `src/dkef-vue/src/components/AboutComponent.vue`
- `src/dkef-vue/src/components/HomeComponent.vue`
- `src/dkef-vue/src/components/MemberAdvantages.vue`

The feature should also accommodate text in other existing or future components without introducing a separate persistence model or bespoke editor for each component.

This document describes the intended functional result and design constraints. It is not a detailed implementation plan; example identifiers and API shapes are illustrative.

## Intended functional result

- Visitors see the current published wording within the existing website design.
- Administrators see a text-editing action for the relevant page or content group.
- The editor presents named fields with Danish labels, organized into understandable sections rather than exposing technical keys.
- Short fields use ordinary inputs; longer descriptions use textareas.
- Administrators can edit multiple fields and save them together. A successful save publishes the changes immediately and updates the displayed text.
- Cancelling discards unsaved edits. Failed saves retain the edits and show a useful Danish error message.
- Validation prevents invalid or excessively long values from being saved.
- Developers can make additional text editable by defining its fields and connecting the component to the shared text mechanism.

The guiding principle is: **developers define the slots; administrators fill them.**

## Design constraints

### Preserve the deliberately simple application

The website has very little traffic. Reuse its existing Vue frontend, ASP.NET Core API, authentication, PostgreSQL database, and modal patterns. Do not introduce a full CMS, separate content service, page builder, or distributed cache.

Saving publishes immediately. Drafts, approval workflows, scheduling, and revision history are outside the initial scope.

### Text is editable; structure is not

Vue components continue to own layout, styling, element types, navigation behavior, and section composition. Administrators cannot:

- Add, remove, or reorder sections, cards, or list entries.
- Change styling, fonts, colors, or layout.
- Supply HTML, scripts, or arbitrary component markup.
- Create arbitrary content fields or keys.

Existing list entries may have editable text, but the number and order of entries remain developer-controlled. Text is rendered as plain text through Vue interpolation, not `v-html`. Where multiline text is supported, line breaks are preserved through presentation rules owned by the component.

Images remain unchanged in this text-focused feature. Future image editing may reuse the grouping and editor conventions, but image upload, storage, and lifecycle decisions are deferred.

### Only deliberately selected text is editable

Not every UI string needs to become editable. Developers choose which headings, descriptions, labels, and other values belong in the feature.

Displayed prices and statistics need particular care: editing a displayed membership price must not be mistaken for changing an underlying fee or business rule. Values derived from application data should not gain an independent editable copy without an explicit reason.

### Enforce permissions and validation on the server

Published text is publicly readable. Updates require the existing `Admin` role on the API; hiding the edit action in the frontend is not sufficient authorization.

The API accepts only registered groups and keys and applies field-specific validation, including required values and length limits. It follows the application's DTO sanitization conventions. Frontend validation improves usability but does not replace server enforcement.

Length limits should reflect the existing layout, especially fixed-height areas. Plain-text editing prevents markup changes but does not by itself prevent text overflow.

## Extensible content model

### Stable groups and semantic keys

Use a generic text store with a group and key identifying each value. For example:

| Group | Key | Purpose |
| --- | --- | --- |
| `home` | `hero.title` | Home-page main heading |
| `home` | `network.description` | Home-page network description |
| `about` | `introduction.description` | Association introduction |
| `member-advantages` | `network.title` | Network benefit heading |

Identifiers describe content, not Vue filenames or incidental array positions. Renaming a component should not disconnect it from saved text. Changing a content identifier later requires an explicit decision about preserving its existing override.

A small `SiteTexts` table in the existing database can store:

- `Group`
- `Key`
- `Value`
- `UpdatedAt`
- `UpdatedBy`

The group/key pair is unique. This data is separate from the event/news/general-assembly inheritance model, even if it uses the existing `ContentsContext`.

New groups and fields do not require new database columns or tables.

### Developer-defined field definitions

Definitions maintained in code describe the available fields, including:

- Stable group and key.
- Default wording.
- Danish editor label and section.
- Single-line or multiline input.
- Required/optional status and maximum length.
- Additional validation where appropriate, such as email formatting.

The same definitions inform the editor and the approved-field contract enforced by the API. The exact sharing mechanism remains an implementation decision: a shared JSON definition file or modest duplication between frontend definitions and backend validation are both possible. A code-generation framework is not a requirement.

### Defaults and overrides

Keep the current wording as developer-maintained defaults and store administrator changes as overrides.

- No stored entry means use the default value.
- An empty string is distinct from a missing override and is accepted only for optional fields.
- A saved override continues to take precedence if the default wording changes in a later deployment.
- If a restore-default action is included, it explicitly removes the override rather than saving an empty value.

Defaults keep the public page readable if fetching text fails. The editor must distinguish a failed load from a successful response containing no overrides; it should require a successful load before allowing saves.

### Deliberate sharing between components

Different components may read from the same group when they intentionally share wording. For example, the repeated membership benefits in Home and Member Advantages could use a shared `membership` group.

Sharing is a content decision, not automatic deduplication of identical strings. Text with different editorial purposes should retain separate keys even if its initial wording matches.

## Reusable application behavior

### Public display

A shared frontend mechanism fetches text by group and merges overrides with defaults. Components bind their existing text slots to the resulting values while retaining their templates and styling.

Fetch a group in one request rather than issuing one request per field. In-memory reuse of loaded groups is sufficient for the expected traffic; persistent browser caching and more elaborate cache infrastructure are unnecessary initially.

### Administrator editor

A reusable editor built on `BaseModal.vue` renders the registered fields. Logical sections keep larger pages such as Home manageable. The editor uses local working values so cancelling does not change published text.

Successful saves update the shared frontend state, including any mounted components that intentionally use the same group. No real-time cross-browser synchronization is required initially.

### API boundary

A small generic API surface can provide public reads and admin-only updates, for example:

```text
GET   /SiteTexts/{group}
PATCH /SiteTexts/{group}
```

An update contains changed key/value pairs. Omitted fields remain unchanged, and all submitted changes are validated and saved together in one transaction. New fields do not require a new controller or endpoint.

The existing frontend API client and URL service remain the integration points. The exact request/response contract and simultaneous-edit behavior will be settled in the implementation plan.

## Initial scope and success criteria

The first delivery applies the reusable mechanism to the selected text in About, Home, and Member Advantages. The editable-field inventory and any deliberately shared wording should be agreed before implementation.

The feature is successful when:

- An administrator can update selected text in all three components without a deployment.
- Visitors see saved text after loading the page, and the administrator sees a successful save immediately.
- Non-admin users cannot update text through the API.
- Invalid fields or values are rejected without partially applying an update.
- Cancelled or failed edits do not change published text.
- Missing overrides and public fetch failures leave readable default content.
- Existing layout, images, navigation, and component structure remain developer-controlled.
- Adding editable text to another component requires field definitions and component bindings, not another database schema, controller, or bespoke form.

Detailed file changes, migration steps, request contracts, and test cases belong in a subsequent implementation plan.
