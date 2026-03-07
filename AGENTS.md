# AGENTS.md — Coding Agent Reference for dkef

## Repository Overview

Monorepo with three components:
- `src/dkef-vue/` — Vue 3 SPA (TypeScript, Vite, Tailwind CSS)
- `src/dkef-api/` — ASP.NET Core Web API (C#, .NET 10, EF Core, PostgreSQL)
- `src/dkef-data/` — .NET 9 console utility (CSV import)
- `automation/` — Ansible playbooks for Kubernetes/KinD cluster provisioning

---

## Build, Lint, Test Commands

### Frontend (`src/dkef-vue/`)

**Package manager:** Bun (`bun.lockb` lockfile — always use `bun`, never `npm` or `yarn`)

```bash
# Install dependencies
bun install

# Development server
bun dev

# Type-check + production build (runs in parallel)
bun build

# Type-check only
bun vue-tsc --build

# Run all unit tests (watch mode)
bun test:unit

# Run all tests once (CI mode, no watch)
bun vitest run

# Run a single test file
bun vitest run src/components/__tests__/HelloWorld.spec.ts

# Run tests matching a name filter
bun vitest run -t "renders properly"

# Lint and auto-fix
bun lint

# Format source files with Prettier
bun format
```

### Backend (`src/dkef-api/`)

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run in development
dotnet run

# Publish
dotnet publish -c Release

# Apply EF Core migrations (multiple DbContexts — run for each)
dotnet ef database update --context ContactContext
dotnet ef database update --context EventsContext
dotnet ef database update --context ForgotPasswordContext
dotnet ef database update --context RefreshTokenContext
```

### Docker / Full Stack

```bash
# Start API + frontend locally (from src/)
docker compose up

# Build and push Docker images (GitHub Actions manual trigger)
# See .github/workflows/build.yml
```

---

## Frontend Code Style (`src/dkef-vue/`)

### Formatting (Prettier + ESLint)

- **No semicolons** at end of statements
- **Single quotes** for strings
- **100-character** print width
- ESLint uses flat config (`eslint.config.ts`, ESLint 9). Prettier handles formatting; ESLint skips it (`skipFormatting`).
- Run `bun lint` (ESLint auto-fix) then `bun format` (Prettier) before committing.

### TypeScript

- Strict mode is inherited from `@vue/tsconfig/tsconfig.dom.json`
- Always use `import type { ... }` for pure type imports:
  ```ts
  import type { PublishedEvent } from '@/types/events'
  ```
- Avoid `any`; use `unknown` and narrow explicitly.
- Enum pattern:
  ```ts
  enum Sort { None = '', Asc = 'asc', Desc = 'desc' }
  ```

### Imports

- Use the `@/` alias for all internal imports (maps to `src/`):
  ```ts
  import { useAuthStore } from '@/stores/authStore'
  import api from '@/services/apiservice'
  ```
- Use relative imports only within test files or same-directory siblings.

### Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Vue components | `PascalCase.vue` | `MembersComponent.vue`, `BaseModal.vue` |
| View components | `PascalCase` + `View` suffix | `MembersView.vue` |
| Pinia stores | `camelCase` + `Store` suffix | `authStore.ts`, export `useAuthStore` |
| TypeScript types/interfaces | `PascalCase` | `PublishedEvent`, `ContactDto` |
| Functions and variables | `camelCase` | `isLoading`, `fetchMembers` |
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
- Views are thin wrappers — all logic lives in feature components.
- Local state uses `ref<T>` and `computed`. Cross-component state uses Pinia stores.

### Error Handling (Frontend)

- Use `try/catch/finally` for all async operations.
- Type the catch variable as `unknown`, narrow explicitly:
  ```ts
  try {
    await api.post('/endpoint', payload)
  } catch (error: unknown) {
    if (error instanceof Error) console.error(error.message)
  } finally {
    isLoading.value = false
  }
  ```
- Track loading state with `ref<boolean>` flags (`isLoading`, `isLoggingIn`).
- Use `AbortController` for cancellable async sequences in components.

### Pinia Stores

- Prefer Composition API store style (`setup` function):
  ```ts
  export const useAuthStore = defineStore('auth', () => {
    const user = ref<User | null>(null)
    // ...
    return { user }
  }, { persist: { key: 'auth', pick: ['accessToken', 'refreshToken', 'user'] } })
  ```
- Use `pinia-plugin-persistedstate` for auth; specify `pick` to limit persisted keys.

### API Service

- All HTTP calls go through `@/services/apiservice.ts` (Axios singleton with JWT interceptors).
- The interceptor handles 401 → refresh token → retry queue (prevents parallel refresh races).
- URL selection is handled by `@/services/urlservice.ts` which switches on `VITE_MODE` for mock vs. real endpoints.
- Import the api object as default: `import api from '@/services/apiservice'`

---

## Backend Code Style (`src/dkef-api/`)

### Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Controllers | `PascalCase` + `Controller` | `ContactsController` |
| Repositories | `PascalCase` + `Repository` / `IRepository` | `ContactRepository`, `IContactRepository` |
| Services | `PascalCase` + `Service` / `IService` | `JwtService`, `IJwtService` |
| Domain models | `PascalCase` | `Contact`, `Event`, `RefreshToken` |
| DTOs | `PascalCase` + `Dto` | `ContactDto`, `LoginDto` |
| Config records | `sealed record`, `PascalCase` | `JwtConfig` |
| Private fields | `_camelCase` prefix | `_repository`, `_mapper` |
| Properties | `PascalCase` | `FirstName`, `CreatedAt` |

### C# Patterns

- Use C# 12 **primary constructors** for dependency injection:
  ```csharp
  public class JwtService(JwtConfig _jwtConfig, IRepository<RefreshToken, Guid> _tokenRepo) : IJwtService
  ```
- String properties default to `string.Empty` (never `null`) — nullable reference types are enabled.
- Controllers return `IActionResult` with typed helpers: `Ok()`, `BadRequest()`, `NotFound()`, `Unauthorized()`, `CreatedAtAction()`.
- Validate GUIDs before processing: `if (!Guid.TryParse(id, out var guid)) return BadRequest();`
- Read-only queries use `AsNoTracking()` consistently.
- Sanitize all incoming DTO string fields with `HtmlSanitizer` before persistence.
- Development-only endpoints are gated with `#if DEBUG`.

### Architecture Patterns

- **Repository pattern**: generic `IRepository<T, Y>` base; specialized per-domain interfaces.
- **Multiple DbContexts**: `ContactContext` (also Identity), `EventsContext`, `ForgotPasswordContext`, `RefreshTokenContext` — each migrated independently.
- **`Contact` extends `IdentityUser`** — member management and authentication share one table.
- **Sortable system**: Use `[Sortable]` attribute on domain properties + `QueryableService<T>` for type-safe dynamic LINQ ordering.
- **Paginated results**: wrap collections in `DomainCollection<T>` (contains `Collection` + `Total`).
- **AutoMapper**: use for DTO↔Domain mapping; do not map manually in controllers.

### Error Handling (Backend)

- Repositories throw `KeyNotFoundException` when an entity is not found.
- JWT service throws `UnauthorizedAccessException` / `InvalidOperationException` on token errors.
- Controllers catch typed exceptions and map to appropriate HTTP status codes — no global exception middleware.
- Startup is wrapped in `try/catch/finally` with `Log.Fatal()` (Serilog).

---

## Testing Conventions

- **Framework:** Vitest 3 + `@vue/test-utils` 2
- **Environment:** jsdom
- **Location:** `src/dkef-vue/src/components/__tests__/`
- **File naming:** `<ComponentName>.spec.ts`
- Test structure:
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

Frontend environment variables are Vite-style (`VITE_` prefix):
- `VITE_API_BASE_URL` — backend API base URL
- `VITE_MODE` — controls URL service routing (`Development` → mock data, else real API)

Backend configuration is via `appsettings.json` / `appsettings.Development.json`:
- `JwtConfig` (secret, issuer, audience, expiry)
- `ConnectionStrings` (PostgreSQL)
- MinIO connection settings

---

## CI/CD

- **GitHub Actions** (`.github/workflows/build.yml`): manually triggered (`workflow_dispatch`); builds and pushes Docker images for both API and frontend to Docker Hub.
- **Dockerfiles**: `src/dkef-api/Dockerfile` (SDK 9 build → ASP.NET 9 runtime), `src/dkef-vue/Dockerfile` (Bun + Vite build → nginx:alpine serve).
- **Compose**: `src/docker-compose.yaml` runs both services on `dkef-network` (API: 5275, frontend: 5173).
