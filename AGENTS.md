# AGENTS.md — dkef

Use this file for repo-level guardrails; for implementation details read the package guides:
- `src/dkef-vue/AGENTS.md`
- `src/dkef-api/AGENTS.md`

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

- `.github/workflows/build.yml` is manual (`workflow_dispatch`) and builds/pushes multi-arch (`linux/amd64`, `linux/arm64`) Docker images for API and frontend.
- Frontend Docker build depends on `ARG BUILD_MODE` and runs `bun run build:${BUILD_MODE}` (`docker` and `k8s` modes are expected).
