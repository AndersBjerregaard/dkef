# AGENTS.md — dkef

Use this file for repo-level guardrails; for implementation details read the package guides:
- `src/dkef-vue/AGENTS.md`
- `src/dkef-api/AGENTS.md`

## Agent contribution workflow

Read [contributing.md](contributing.md) before making changes. It defines the goal-driven workflow, tool usage, validation, and human-review handoff.

- Establish the goal, acceptance criteria, and relevant package guidance before editing; keep changes focused and preserve unrelated user work.
- Never modify, commit to, or push directly to the default branch. Discover the default branch from GitHub rather than assuming its name.
- Always implement feature requests on a feature branch based on the updated default branch. An existing task branch may be reused if it has the correct base and scope.
- Validate the affected packages, review the diff, and commit only task-related files. When finished, push the feature branch and open a PR against the default branch for human review; do not merge it yourself.
- Prefer GitHub MCP for repository operations. If it lacks authorization for an operation, use the authenticated GitHub CLI (`gh`) instead. Neither tool may bypass branch protection or the human-review requirement; report a blocker if both lack access.
- Use `dotnet`, Bun (instead of Node.js/npm/yarn workflows), `python3` (agent scripting only, not application source), `dotnet ef`, Docker, GitHub CLI, and GitHub MCP as described in the contribution guide.
- Use the globally available `playwright-cli` with the preinstalled Chromium browser for focused frontend validation. Prefer headless (the default); see [the browser validation workflow](contributing.md#frontend-browser-validation) for commands and safety boundaries.
- 🚨 Do not run Ansible playbooks directly. Deployment/provisioning is a human-operated handoff, not part of completing a source-code contribution.
- Confirm local targets before running database-mutating commands or starting the API, which automatically applies migrations. Do not change shared/production data or infrastructure without explicit authorization.

## Monorepo map

- `src/dkef-vue` - Vue 3 + Vite + Bun frontend
- `src/dkef-api` - ASP.NET Core Web API (`net10.0`)
- `src/dkef-data` - interactive .NET console utility for CSV import/update
- `automation/ansible` + `automation/k8s` - deployment/provisioning (KinD + Ansible)

## Commands you are likely to need

- Frontend (`src/dkef-vue`): `bun install`, `bun dev`, `bun build`, `bun lint`, `bun format`, `bun vitest run`, `bun vitest run src/components/__tests__/SomeFile.spec.ts`
- Backend (`src/dkef-api`): `dotnet restore`, `dotnet build`, `dotnet run`
- Data utility (`src/dkef-data`): `dotnet run` (interactive; expects CSV files under `src/dkef-data/data`, which is gitignored)
- Infra notes: there is no repo-level compose stack; the only compose file is `src/dkef-data/db/docker-compose.yaml` (Postgres + MinIO only)

## High-signal gotchas

- Frontend package manager is Bun (`bun.lockb` exists); do not use npm/yarn.
- Bun 1.3.x can load `.env.development` unexpectedly when using script aliases like `bun run staging`; if mode behavior looks wrong, run Vite directly (example: `bun --env-file=.env.staging vite --mode staging`) or use Bun >= 1.4.
- Frontend user-facing strings must be Danish (keep comments/logs in English if needed).
- Backend loads optional overrides from `src/dkef-api/appsettings.Local.json` in addition to default appsettings files.
- Backend auto-runs EF migrations on startup for all six contexts in `Program.cs`: `ContactsContext`, `AspNetRolesContext`, `ContentsContext`, `ForgotPasswordContext`, `ChangeEmailContext`, `RefreshTokensContext`.

## CI/build facts worth knowing

- `.github/workflows/build.yml` supports release tag pushes and manual runs (`workflow_dispatch`), and builds/pushes `linux/amd64` Docker images for API and frontend.
- Workflow publishes images only; production rollout is manual via `automation/ansible`.
- Frontend Docker build depends on `ARG BUILD_MODE` and runs `bun run build:${BUILD_MODE}` (`docker` and `k8s` modes are expected).
