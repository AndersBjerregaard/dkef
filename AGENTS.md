# AGENTS.md — Coding Agent Reference for dkef

## Supplementary Context

Detailed per-package context is available in subdirectory `.opencode/AGENTS.md` files.
For frontend tasks, read `src/dkef-vue/.opencode/AGENTS.md` (components, views, stores, routing, theming, URL service).
For backend tasks, read `src/dkef-api/.opencode/AGENTS.md` (controllers, domain models, DTOs, repositories, services, DbContexts).
These files are also loaded automatically via `.opencode/opencode.json`.

---

## Repository Overview

Monorepo with three components:
- `src/dkef-vue/` — Vue 3 SPA (TypeScript, Vite, Tailwind CSS)
- `src/dkef-api/` — ASP.NET Core Web API (C#, .NET 10, EF Core, PostgreSQL)
- `src/dkef-data/` — .NET 9 console utility (CSV import)
- `automation/` — Ansible playbooks and Kubernetes manifests for Kubernetes/KinD cluster provisioning

---

## Build, Lint, Test Commands

### Frontend (`src/dkef-vue/`)

**Run all frontend commands from `src/dkef-vue/`.**  
**Package manager:** Bun (`bun.lockb` binary lockfile — always use `bun`, never `npm` or `yarn`, never edit `bun.lockb` manually)

```bash
bun install                     # Install dependencies
bun dev                         # Development server
bun run staging                 # Dev server in staging mode
bun build                       # Type-check + production build (parallel)
bun run build:staging           # Build for staging
bun run build:docker            # Build for Docker (uses .env.docker)
bun run build:k8s               # Build for Kubernetes (uses .env.k8s)
bun vue-tsc --build             # Type-check only
bun lint                        # ESLint auto-fix
bun format                      # Prettier format (src/ only)

# Tests
bun test:unit                   # Run all unit tests (watch mode)
bun vitest run                  # Run all tests once (CI mode)
bun vitest run src/components/__tests__/HelloWorld.spec.ts  # Single file
bun vitest run -t "renders properly"                        # By name filter
```

### Backend (`src/dkef-api/`)

**Run all backend commands from `src/dkef-api/`.**

```bash
dotnet restore
dotnet build
dotnet run                      # Run in development

# Apply EF Core migrations (one command per DbContext)
dotnet ef database update --context ContactContext
dotnet ef database update --context EventsContext
dotnet ef database update --context NewsContext
dotnet ef database update --context GeneralAssemblyContext
dotnet ef database update --context ForgotPasswordContext
dotnet ef database update --context RefreshTokenContext
```

### Docker / Full Stack

```bash
# From src/ — starts API, frontend, PostgreSQL, pgAdmin (port 8080), and MinIO (ports 9000/9001)
docker compose up

# GitHub Actions manual trigger builds and pushes Docker images to Docker Hub
# See .github/workflows/build.yml
```

---

## Frontend Code Style (`src/dkef-vue/`)

> **Language:** All user-facing UI text must be hard-coded in Danish. Comments and logs may be in English.

### Formatting

- **No semicolons**, **single quotes**, **100-character** print width (`.prettierrc.json`)
- ESLint flat config (`eslint.config.ts`, ESLint 9) — Prettier handles formatting via `skipFormatting`
- Always run `bun lint` then `bun format` before committing

### TypeScript

- Strict mode via `@vue/tsconfig/tsconfig.dom.json`
- Use `import type { ... }` for pure type imports
- Avoid `any`; use `unknown` and narrow explicitly
- Enum pattern: `enum Sort { None = '', Asc = 'asc', Desc = 'desc' }`

### Imports

- Use the `@/` alias for all internal imports (maps to `src/`):
  ```ts
  import { useAuthStore } from '@/stores/authStore'
  import api from '@/services/apiservice'
  import type { PublishedEvent } from '@/types/events'
  ```
- Use relative imports only in test files or same-directory siblings

### Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Vue components | `PascalCase.vue` | `MembersComponent.vue`, `BaseModal.vue` |
| Views | `PascalCase` + `View` suffix | `MembersView.vue` |
| Pinia stores | `camelCase` + `Store` suffix | `authStore.ts`, exports `useAuthStore` |
| Types/interfaces | `PascalCase` | `PublishedEvent`, `ContactDto` |
| Functions/variables | `camelCase` | `isLoading`, `fetchMembers` |
| Service modules | `camelCase` | `apiservice.ts`, `urlservice.ts` |

### Vue Component Style

- Always use `<script setup lang="ts">` (Composition API):
  ```vue
  <script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import type { ContactDto } from '@/types/members'

  const props = defineProps<{ msg: string }>()
  const emit = defineEmits<{ (e: 'update', val: string): void }>()
  </script>
  ```
- Views (`*View.vue`) are thin wrappers — all logic lives in feature components
- Local state: `ref<T>` and `computed`. Cross-component state: Pinia stores

### Error Handling

```ts
try {
  await api.post('/endpoint', payload)
} catch (error: unknown) {
  if (error instanceof Error) console.error(error.message)
} finally {
  isLoading.value = false
}
```
- Track loading state with `ref<boolean>` flags (`isLoading`, `isLoggingIn`)
- Use `AbortController` for cancellable async sequences

### Pinia Stores

- Composition API store style with `pinia-plugin-persistedstate`; specify `pick` to limit persisted keys:
  ```ts
  export const useAuthStore = defineStore('auth', () => {
    const user = ref<User | null>(null)
    return { user }
  }, { persist: { key: 'auth', storage: localStorage, pick: ['accessToken', 'refreshToken', 'user'] } })
  ```

### API Service

- All HTTP calls go through `@/services/apiservice.ts` (Axios singleton)
- The interceptor handles 401 → refresh token → retry queue (prevents parallel refresh races)
- API base URL is set via `VITE_API_BASE_URL` env var; import as default:
  ```ts
  import api from '@/services/apiservice'
  await api.get('/events')
  await api.post('/events', payload)
  await api.put('/events/id', payload)
  await api.delete('/events/id')
  ```

---

## Backend Code Style (`src/dkef-api/`)

### Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Namespace | `Dkef.*` | `Dkef.Controllers`, `Dkef.Domain` |
| Controllers | `PascalCase` + `Controller` | `EventsController` |
| Repositories | `PascalCase` + `Repository` / `IRepository` | `EventsRepository`, `IEventsRepository` |
| Services | `PascalCase` + `Service` / `IService` | `JwtService`, `IJwtService` |
| Domain models | `PascalCase`, extend `DomainClass` (has `Guid Id`) | `Event`, `News` |
| DTOs (Contracts) | `PascalCase` + `Dto` | `EventDto`, `LoginDto` |
| Config records | `sealed record`, `PascalCase` | `JwtConfig` |
| Private fields | `_camelCase` prefix | `_repository`, `_mapper` |

### C# Patterns

- **Primary constructors** for dependency injection (C# 12):
  ```csharp
  public class EventsController(IEventsRepository _repository, IMapper _mapper,
      HtmlSanitizer _sanitizer, QueryableService<Event> _queryableService) : ControllerBase
  ```
- `Nullable` and `ImplicitUsings` are enabled project-wide
- String properties default to `string.Empty` — never `null`
- Controllers return `IActionResult`: `Ok()`, `BadRequest()`, `NotFound()`, `Unauthorized()`, `CreatedAtAction()`
- Validate GUIDs: `if (!Guid.TryParse(id, out var parsedId)) return BadRequest(...);`
- Read-only queries use `.AsNoTracking()` consistently
- Sanitize all incoming DTO string fields with `HtmlSanitizer` before persistence
- Development-only endpoints gated with `#if DEBUG`

### Architecture Patterns

- **Repository pattern**: generic `IRepository<T, Y>` base; specialized per-domain interfaces
- **6 DbContexts**: `ContactContext` (also ASP.NET Identity), `EventsContext`, `NewsContext`, `GeneralAssemblyContext`, `ForgotPasswordContext`, `RefreshTokenContext` — each migrated independently
- **`Contact` extends `IdentityUser`** — member management and authentication share one table
- **RBAC**: write endpoints use `[Authorize(Roles = "Admin")]`; public read endpoints are unauthenticated
- **Sortable system**: `[Sortable]` attribute on domain properties + `QueryableService<T>` for type-safe dynamic LINQ ordering via `System.Linq.Dynamic.Core`
- **Paginated results**: `DomainCollection<T>` wrapper (has `Collection` + `Total`)
- **AutoMapper**: DTO↔Domain mapping configured in `Program.cs`; do not map manually in controllers
- **`Bogus`** package is available for generating test/seed data

### Error Handling

- Repositories throw `KeyNotFoundException` when an entity is not found
- JWT service throws `UnauthorizedAccessException` / `InvalidOperationException` on token errors
- Controllers catch typed exceptions and map to HTTP status codes — no global exception middleware
- Startup is wrapped in `try/catch/finally` with `Log.Fatal()` (Serilog)

---

## Testing Conventions

- **Framework:** Vitest 3 + `@vue/test-utils` 2, environment: jsdom
- **Location:** `src/dkef-vue/src/components/__tests__/`
- **File naming:** `<ComponentName>.spec.ts`

```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MyComponent from '../MyComponent.vue'

describe('MyComponent', () => {
  it('renders the title prop', () => {
    const wrapper = mount(MyComponent, { props: { title: 'Hello' } })
    expect(wrapper.text()).toContain('Hello')
  })
})
```

---

## Environment Variables

Frontend (`VITE_` prefix, Vite-style):
- `VITE_API_BASE_URL` — backend API base URL
- `VITE_MODE` — mode label (e.g. `Development`, `Docker`, `k8s`)

Backend (`appsettings.json` / `appsettings.Development.json`):
- `JwtSettings` — `Key`, `Issuer`, `Audience`, `ExpiryMinutes`
- `ConnectionStrings:PostgresDb` — PostgreSQL connection string
- `ConnectionStrings:Minio` + `Minio:AccessKey`, `Minio:SecretKey`, `Minio:Secure`

---

## CI/CD

- **GitHub Actions** (`.github/workflows/build.yml`): manually triggered (`workflow_dispatch`); builds and pushes multi-arch (`linux/amd64`, `linux/arm64`) Docker images to Docker Hub
- **API Dockerfile**: `dotnet/sdk:10.0` build → `dotnet/aspnet:10.0` runtime
- **Frontend Dockerfile**: `oven/bun:1.3.10` build → `nginx:alpine` serve
- **Compose** (`src/docker-compose.yaml`): API (5275), frontend (5173), PostgreSQL, pgAdmin (8080), MinIO (9000/9001)
