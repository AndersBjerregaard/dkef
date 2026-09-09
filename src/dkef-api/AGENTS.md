# Backend Supplementary Context — `src/dkef-api/`

## Project Layout

```
src/dkef-api/
├── Attributes/      [Sortable] attribute for dynamic LINQ ordering
├── Configuration/   JwtConfig (sealed record), SortablePropertyConfig
├── Contracts/       DTOs (PostObject base + per-domain Dtos) and Validation/
├── Controllers/     One controller per domain
├── Data/            DbContext classes (one per domain)
├── Domain/          Domain models (extend DomainClass → Guid Id)
├── Endpoints/       Minimal API endpoints (BaseEndpoints, ContactEndpoints)
├── Repositories/    Concrete repos + Interfaces/
├── Services/        JwtService, MinioBucketService, QueryableService, DatabaseTokenProvider
└── Program.cs       Composition root — all DI wiring, AutoMapper config, middleware
```

---

## Controller Inventory

| Controller | Route prefix | Auth |
|---|---|---|
| `AuthController` | `/auth` | Public (all endpoints) |
| `BucketController` | `/bucket` | Admin only |
| `ContactsController` | `/contacts` | Admin only (entire controller) |
| `EventsController` | `/events` | GET public; POST/PUT Admin |
| `FeedController` | `/feed` | Public |
| `GeneralAssembliesController` | `/general-assemblies` | GET public; write Admin |
| `NewsController` | `/news` | GET public; write Admin |

**Auth endpoints** (`AuthController`): `POST /auth/login`, `POST /auth/register`,
`POST /auth/refresh`, `POST /auth/logout`, `POST /auth/forgot`, `POST /auth/reset`,
`POST /auth/change`. `GET /auth/forgot/{id}` is Admin-only (token inspection).

**`ContactsController`** is fully gated with `[Authorize(Roles = "Admin")]` at the class level —
there are no public contact endpoints. It also has a `POST /contacts/seed` endpoint compiled
only in `#if DEBUG`.

All write endpoints sanitize incoming DTO string fields via `dto.Sanitize(_sanitizer)`
(`HtmlSanitizer`) before any persistence.

---

## Domain Model Inventory

All domain models except `Contact` extend `DomainClass` (provides `public Guid Id`).

| Model | Context | Notable fields |
|---|---|---|
| `Contact` | `ContactContext` | Extends `IdentityUser`; 30+ profile fields mapping to CSV import schema (name, address, company, CVR, sections, etc.) |
| `Event` | `EventsContext` | `Title`, `Section`, `Address`, `DateTime`, `Description`, `ThumbnailUrl`, `CreatedAt`; `[Sortable]` on `DateTime` and `CreatedAt` |
| `News` | `NewsContext` | Similar shape to `Event` |
| `GeneralAssembly` | `GeneralAssemblyContext` | Similar shape to `Event` |
| `ForgotPassword` | `ForgotPasswordContext` | `ContactId`, `IsUsed`, `IsValid`, token GUID |
| `RefreshToken` | `RefreshTokenContext` | Stores issued refresh tokens for revocation |
| `FeedItem` | — | Aggregated view model returned by `FeedController` |

`Contact` is the ASP.NET Identity user (`IdentityUser` subclass). `ContactContext` doubles as the
Identity store. The `Contact` model fields were imported from a legacy CSV — do not remove or
rename existing fields.

---

## DTO and Validation Patterns

All DTOs extend `PostObject`, which requires implementing `Sanitize(HtmlSanitizer sanitizer)`.
Every string field that could contain user input must be sanitized inside this method.

Custom validation attributes live in `Contracts/Validation/`:
- `[DateTimeValidation]` — validates ISO 8601 / parseable date-time strings
- `[GuidValidation]` — validates that a string is a valid GUID

Use `[Required(AllowEmptyStrings = false)]` on all mandatory DTO string fields. Default all
string properties to `string.Empty`, never `null`.

---

## Repository Patterns

All repositories implement `IRepository<T, Y>` and are registered as `Scoped`.

Key implementation details:
- **Read queries** always call `.AsNoTracking()` for performance.
- **`GetMultipleAsync`** always returns `DomainCollection<T>` (has `Collection` + `Total`).
  The `Total` comes from a separate `CountAsync()` call on the unfiltered set.
- **`UpdateAsync`** fetches the tracked entity then calls `mapper.Map(dto, existing)` to merge
  changes in-place — it does **not** replace the entity. Throws `KeyNotFoundException` if the
  entity does not exist (controllers catch this and return `NotFound()`).
- **`DeleteAsync`** uses `ExecuteDeleteAsync()` for a direct SQL `DELETE` (no load-then-delete).

Sorting is handled externally: `QueryableService<T>` validates the field name against
`[Sortable]`-attributed properties via `SortablePropertyConfig`, then produces an
`IOrderedQueryable<T>` using `System.Linq.Dynamic.Core`. Pass this to the overload of
`GetMultipleAsync` that accepts `IOrderedQueryable<T>`.

---

## Services

- **`JwtService`** — issues access + refresh tokens (`GenerateTokensAsync`), validates and
  rotates refresh tokens (`RefreshTokenAsync`), revokes tokens (`RevokeRefreshTokenAsync`).
  Throws `UnauthorizedAccessException` for invalid/expired tokens.
- **`MinioBucketService`** — generates presigned PUT URLs for client-side direct upload to MinIO.
  Buckets are named `events`, `news`, `general-assemblies`.
- **`DatabaseTokenProvider`** — custom ASP.NET Identity token provider that persists password
  reset tokens to `ForgotPasswordContext` instead of using the default in-memory provider.
  Registered as `"DatabaseTokenProvider"` in Identity options.
- **`QueryableService<T>`** — see Repository Patterns above.

---

## DbContext → Migration Reference

| Context | Owns | Notes |
|---|---|---|
| `ContactContext` | `Contact` | Also the Identity store (`AddIdentity<Contact, IdentityRole>`) |
| `EventsContext` | `Event` | |
| `NewsContext` | `News` | |
| `GeneralAssemblyContext` | `GeneralAssembly` | |
| `ForgotPasswordContext` | `ForgotPassword` | |
| `RefreshTokenContext` | `RefreshToken` | |

All six contexts are migrated automatically on startup via `database.Migrate()` in `Program.cs`.
When adding a new migration, specify the context explicitly:
`dotnet ef migrations add <Name> --context <ContextName>`

---

## Development Tooling

- **Scalar API explorer** is available at `/scalar/v1` when running in Development mode —
  use this for manual endpoint testing instead of external tools.
- **`appsettings.Development.json`** overrides connection strings and JWT settings locally.
- The `.env` file in `src/dkef-api/` is loaded by Docker Compose for container configuration.
