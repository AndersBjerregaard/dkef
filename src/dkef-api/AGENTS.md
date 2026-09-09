# AGENTS.md - src/dkef-api

## Fast commands (run from `src/dkef-api`)

- Restore/build/run: `dotnet restore`, `dotnet build`, `dotnet run`
- Use launch profile when needed: `dotnet run --launch-profile http` (binds `http://localhost:5275`)
- There is no test project under `src/dkef-api` right now; verify changes with focused API calls against local run.

## Wiring that matters

- Real composition root is `Program.cs`; keep DI, auth, mapper, and middleware changes there.
- `Endpoints/` exists but is not mapped; live HTTP surface is controller-based via `app.MapControllers()`.
- `ContactsContext` is the ASP.NET Identity store (`IdentityDbContext<Contact>`); `Contact.Id` is Identity string GUID.
- Content is single-table inheritance in `ContentsContext` (`Contents` with discriminator `ContentType` for `Event`/`News`/`GeneralAssembly`).

## High-signal gotchas

- Startup auto-runs `Database.Migrate()` for six contexts: `ContactsContext`, `AspNetRolesContext`, `ContentsContext`, `ForgotPasswordContext`, `ChangeEmailContext`, `RefreshTokensContext`.
- Because of multiple contexts, always create migrations with explicit context and output folder, e.g. `dotnet ef migrations add <Name> --context ContentsContext --output-dir Migrations/Contents`.
- `Program.cs` always loads optional `appsettings.Local.json` in addition to environment settings.
- Email service is switchable by config: `UseSmtp=true` uses `MicrosoftGraphEmailService`; default is `DevelopmentEmailService`.
- MinIO admin calls use a keyed internal endpoint (`ConnectionStrings:MinioInternal`, fallback `ConnectionStrings:Minio`); preserve this split when touching bucket logic.

## API behavior conventions to keep

- DTO sanitization is required: write endpoints call `dto.Sanitize(HtmlSanitizer)` before mapping/persistence.
- `QueryableService<T>` only accepts fields marked sortable and only `asc`/`desc`; invalid query params throw.
- Debug-only maintenance endpoints are compiled under `#if DEBUG` (`POST /Contacts/seed`, `POST /ImageCleanup/cleanup`); do not rely on them for production behavior.
- In Development, OpenAPI + Scalar are enabled (`/openapi/*` and Scalar UI via `MapScalarApiReference`).
