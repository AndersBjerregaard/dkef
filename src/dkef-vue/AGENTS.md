# AGENTS.md - src/dkef-vue

## Commands that matter (run from `src/dkef-vue`)

- Install: `bun install`
- Dev server: `bun dev`
- Full build check: `bun run build` (runs `type-check` then Vite build)
- Lint/format: `bun lint`, `bun format`
- Tests: `bun test:unit`
- Single test file: `bun vitest run src/components/__tests__/HelloWorld.spec.ts`

## Environment and build gotchas

- Use Bun, not npm/yarn (`bun.lockb` is source of truth).
- Bun 1.3.x can load `.env.development` when using script aliases with modes; for staging use direct Vite invocation: `bun --env-file=.env.staging vite --mode staging`.
- Docker image build runs `bun run build:${BUILD_MODE}` with `BUILD_MODE` limited to `docker` or `k8s` in `Dockerfile`; default is `k8s`.
- `VITE_MODE=Development` is mock-like only: in `src/services/urlservice.ts`, most endpoints throw `Unimplemented!` (only contacts GET returns `/contacts.json`). Use non-development mode for API-backed flows.

## Wiring and boundaries

- Composition root is `src/main.ts` (Pinia + persisted-state plugin + router + global CSS + `vue-sonner`).
- Route table and auth gating live in `src/router/index.ts`; most views are lazy-loaded.
- Current guard redirects unauthenticated users to `{ name: 'login' }`, but no `login` route exists; login is a modal in `src/App.vue`.
- Keep API path construction in `src/services/urlservice.ts`; do not hardcode endpoint strings in components/stores.
- HTTP client behavior is centralized in `src/services/apiservice.ts` (Bearer injection + token refresh queue); use `skipAuth: true` for public endpoints.

## Conventions to preserve

- All user-facing UI text must be Danish.
- Reuse `src/components/BaseModal.vue` for modal flows (edit/delete/create modals already depend on it).
- Store styles are mixed by design: `authStore`/`feedStore`/`themeStore` use setup-style stores, while `eventStore`/`newsStore`/`generalAssemblyStore` use options-style stores; follow the pattern of the file you edit.
- Theme tokens come from `src/assets/base.css` (`bg-theme-*`, `text-theme-*`, `border-theme-*`); prefer semantic tokens over raw Tailwind colors.

## Testing reality

- Vitest runs in `jsdom` (`vitest.config.ts`).
- Unit tests include the component smoke test, `src/content/__tests__/siteTexts.spec.ts` (registry parity, group coverage, and value rules), `src/stores/__tests__/siteTextStore.spec.ts` (shared text state), and `src/services/__tests__/` (URL mode handling and API authentication/refresh). Most editor/UI behavior is still verified manually.
- For frontend behavior changes, use the globally available `playwright-cli` and preinstalled Chromium for focused browser validation alongside build/lint/unit checks. Prefer headless (default; omit `--headed`). Follow [the contribution guide's browser workflow](../../contributing.md#frontend-browser-validation).
- Run browser commands from the repository root to load `.playwright/cli.config.json`, or pass `--config=../../.playwright/cli.config.json` to `open` from this package. Use a task-specific named session, inspect snapshots/console/requests, check relevant desktop/mobile layouts, and close your session afterward.
- Confirm local frontend/API targets before browser interaction; development mode's limited mocks cannot validate most API-backed flows. Keep browser output and saved authentication state out of commits, and report validation limitations in the PR.
