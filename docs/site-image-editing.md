# Administrator site-image editing

Administrators can replace the ten main images on Home (3), About (2, including the section map) and Member Advantages (5), without rebuilding the frontend. The six company logos on Home are intentionally unchanged.

## Using the editor

Use **Rediger billeder** beside the page's text-editing controls. Each fixed slot shows its current image and **Skift billede** file picker. Selecting a file creates a local preview; it does not upload or publish anything. Supply a short Danish alternative description, then use **Gem billede** to publish that one image. Alternative descriptions are saved with replacements, not edited separately.

**Annuller valg** discards the selection. Failed saves retain the selected file and description for retry. Closing the modal discards unsaved selections. If administrator access is lost, saving is disabled without clearing an open selection. **Gendan standardbillede** asks for confirmation before removing the override and restoring the bundled original.

JPEG, PNG and WebP still images are supported, up to 5 MiB, 16 million pixels and 8192 pixels on either side. The API checks actual contents regardless of filename/MIME, rejects animated PNG/WebP and other formats, applies EXIF orientation, strips metadata (including GPS), and re-encodes to WebP. No cropping tools, image library, history, reordering or adding/removing slots are included. Existing photo aspect ratios, card crops, map containment and the circular portrait remain developer-controlled.

## Implementation

- [siteImages.ts](../src/dkef-vue/src/content/siteImages.ts) defines the UI inventory and bundled defaults. A parity test checks its identifiers against the API's embedded [siteImageSlots.json](../src/dkef-api/Configuration/siteImageSlots.json) allowlist.
- [SiteImage.vue](../src/dkef-vue/src/components/SiteImage.vue) uses the in-memory Pinia store and falls back to the bundled source **and** description when an override fails to load. Failed API reads leave defaults or previously loaded images visible. No overrides are persisted in browser storage.
- Each page's editor uses `BaseModal.vue`, refreshes current overrides before enabling edits, and saves one slot at a time. Successful writes immediately update mounted consumers. Other browsers see changes on a fresh page load; this is not live synchronization.
- API requests use the existing authentication/refresh client and centralized URL service. Development mock mode retains defaults but cannot edit images; API-backed validation requires a non-Development mode pointed at local resources.

### API

| Endpoint | Access | Behavior |
| --- | --- | --- |
| `GET /SiteImages/{group}` | Public | `{ "group": "home", "overrides": { "hero": { "url": "…", "alt": "…" } } }`, with `Cache-Control: no-store`. No audit fields are returned. |
| `PUT /SiteImages/{group}/{slot}` | Admin | Multipart with exactly one `file` and one `alt`; returns the published `{ "url": "…", "alt": "…" }`. |
| `DELETE /SiteImages/{group}/{slot}` | Admin | Removes the override, returns 204, and is idempotent. |

Identifiers are case-sensitive; unknown groups/slots return 404. Writes require the Admin role and an unambiguous authenticated Identity ID, never an identity supplied in the request. Alternative descriptions use the existing plain-text sanitization rules and are required, single-line and at most 300 characters. Request limits apply before multipart buffering, including requests without Content-Length. Public images never need authentication or expiring download links.

### Persistence, storage and failures

`SiteImages` lives in `ContentsContext`, keyed by `(Group, Slot)`, with `ObjectKey`, `Alt`, `UpdatedAt` and `UpdatedBy`. Only replacements are stored: there is no initial seeding or migration of the existing static files. The `AddSiteImages` migration adds only this table; rolling it back deletes overrides, not bundled originals or MinIO objects.

Files use a dedicated public-read `site-images` MinIO bucket and immutable unique keys (`group/slot/guid.webp`). Existing `Minio`/`MinioInternal`/`Minio:PublicEndpoint` configuration and credentials are reused; server-side upload, deletion and bucket operations use the internal endpoint. The API creates the bucket/public-read policy on first upload, so its storage credentials need bucket creation/policy and object read/write/delete permissions. The existing event/news cleanup does **not** scan this bucket.

Uploads finish before the database reference changes. PostgreSQL transaction-scoped advisory locks serialize replacements/resets per slot, including concurrent first uploads. Last committed write wins for the same slot; different slots are independent. The previous committed object is deleted only after a successful database commit. Cleanup failures are safely logged and do not fail publication/reset. A failed or ambiguously acknowledged database commit deliberately leaves the new file in storage, rather than risk deleting a published image.

There is no background orphan sweeper in this version. Failed commits/cleanup can leave unused files. Any later maintenance must compare **full object keys** against `SiteImages.ObjectKey` and allow a grace period for in-flight uploads; do not extend the existing GUID-only event/news cleaner to this bucket. Restoring database backups also requires matching MinIO backups; deleted replacements are not a revision history.

The image decoder is `Magick.NET-Q8-AnyCPU` (Apache-2.0), which bundles its native libraries, including Linux musl builds for the existing Alpine container. No separate ImageMagick installation or external format delegates are required. The decoder is explicitly restricted to JPEG/PNG/WebP input; keep this dependency security-patched.

## Validation and operational handoff

- `bun vitest run`, `bun run build` in `src/dkef-vue`; focused ESLint/Prettier on changed files.
- `dotnet test tests/dkef-api.Tests/dkef-api.Tests.csproj`: image decoding and real loopback HTTP tests run without application startup, a database or MinIO. HTTP tests use in-memory storage/repository boundaries and real JWT authorization/multipart parsing.
- `DKEF_TEST_POSTGRES` enables the isolated PostgreSQL migration, replace/reset and concurrent-first-upload tests using the existing disposable database fixture. See [the contribution guide](../contributing.md#backend-validation) for local-only safety checks and WSL environment forwarding.
- `DKEF_TEST_MINIO=127.0.0.1:<port>`, together with `DKEF_TEST_MINIO_ACCESS_KEY` and `DKEF_TEST_MINIO_SECRET_KEY`, enables the real storage test. Use a fresh disposable local MinIO instance: the test refuses an existing `site-images` bucket, creates its own bucket/object, verifies anonymous download and internal-endpoint upload/deletion, and removes only those test resources. Forward all three variables through `WSLENV` when using Windows-hosted `dotnet.exe`; never commit credentials.
- Browser checks should cover all three page editors on desktop/mobile, anonymous/admin controls, selecting without publishing, failed-save draft preservation, successful publication, reset confirmation and fallback behavior. Browser mocks validate UI behavior, not real storage connectivity.
- The API ingress manifest raises nginx's body limit to **6 MiB**, enough for the 5 MiB file plus multipart overhead. Any additional proxy must allow the same bound; the API still enforces the stricter application limits.

Deployment/provisioning remains human-operated. Apply the reviewed source changes through the normal deployment process, confirm the database/MinIO targets and permissions, and apply the ingress update. API startup automatically applies the `ContentsContext` migration. Do not run Ansible playbooks as contribution validation.
