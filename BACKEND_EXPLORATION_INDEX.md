# Backend Codebase Exploration - Complete Index

**Generated:** March 29, 2026  
**Repository:** dkef (DKEF Member Management System)  
**Scope:** src/dkef-api/ (ASP.NET Core 10 Web API) + Frontend integration  

---

## Quick Navigation

### 1. Summary Documents
- **BACKEND_EXPLORATION_SUMMARY.txt** — Executive summary with all findings (TXT format, easy to read)
- **BACKEND_EXPLORATION_REPORT.md** — Detailed technical report (Markdown format, structured)
- **BACKEND_EXPLORATION_INDEX.md** — This file

### 2. Key Findings at a Glance

#### File Storage
- **Technology:** MinIO (S3-compatible)
- **Buckets:** events, news, general-assemblies
- **Presigned URLs:** 1-hour expiry, HMAC-signed, PUT-only
- **Service:** MinioBucketService (implements IBucketService)
- **Controller:** BucketController (/bucket/{type}/{id})
- **Cleanup:** ImageCleanupService removes orphaned images

#### Data Models
Three entities with identical patterns:
1. **Event** — Title, Section, Address, DateTime, Description, ThumbnailId
2. **News** — Title, Section (opt), Author (opt), Description, ThumbnailId
3. **GeneralAssembly** — Title, Section, Address, DateTime, Description, ThumbnailId

#### Controllers
Standard CRUD endpoints for all three entities:
- GET (public, paginated, sortable)
- POST/PUT/DELETE (Admin role required)

#### Database
- **Provider:** PostgreSQL 14+
- **Structure:** 8 independent DbContexts with separate migrations
- **PK:** UUID (Guid)
- **Timestamps:** UTC timezone

---

## File Locations Reference

### File Storage (MinIO Integration)

```
src/dkef-api/
├── Services/
│   ├── MinioBucketService.cs                (Main implementation)
│   ├── ImageCleanupService.cs               (Orphan image removal)
│   └── Interfaces/
│       └── IBucketService.cs                (Contract)
└── Controllers/
    ├── BucketController.cs                  (Presigned URL endpoints)
    └── ImageCleanupController.cs            (Admin cleanup endpoint)
```

### Data Transfer Objects (DTOs)

```
src/dkef-api/Contracts/
├── EventDto.cs
├── NewsDto.cs
├── GeneralAssemblyDto.cs
├── PostObject.cs                            (Base class)
└── Validation/
    ├── GuidValidationAttribute.cs           (ThumbnailId validation)
    └── DateTimeValidationAttribute.cs       (DateTime validation)
```

### Domain Models

```
src/dkef-api/Domain/
├── Event.cs
├── News.cs
├── GeneralAssembly.cs
├── DomainClass.cs                           (Base class with Guid Id)
└── DomainCollection.cs                      (Pagination wrapper)
```

### Controllers

```
src/dkef-api/Controllers/
├── EventsController.cs                      (/events endpoints)
├── NewsController.cs                        (/news endpoints)
├── GeneralAssembliesController.cs           (/general-assemblies endpoints)
├── BucketController.cs                      (/bucket endpoints)
└── ImageCleanupController.cs                (Admin cleanup)
```

### Repositories

```
src/dkef-api/Repositories/
├── EventsRepository.cs
├── NewsRepository.cs
├── GeneralAssemblyRepository.cs
├── Interfaces/
│   ├── IRepository.cs                       (Generic base)
│   ├── IEventsRepository.cs
│   ├── INewsRepository.cs
│   └── IGeneralAssemblyRepository.cs
```

### Database Contexts & Migrations

```
src/dkef-api/
├── Data/
│   ├── EventsContext.cs
│   ├── NewsContext.cs
│   └── GeneralAssemblyContext.cs
└── Migrations/
    ├── Events/
    │   └── 20260309194134_EventsInitialCreate.cs
    ├── News/
    │   └── 20260309194136_NewsInitialCreate.cs
    └── GeneralAssembly/
        └── 20260309194134_GeneralAssemblyInitialCreate.cs
```

### Frontend Integration

```
src/dkef-vue/src/
├── components/
│   ├── EditEventModal.vue
│   ├── EditNewsModal.vue
│   └── EditGeneralAssemblyModal.vue
├── types/
│   ├── events.ts                            (EventDto, PublishedEvent interfaces)
│   ├── news.ts                              (NewsDto, PublishedNews interfaces)
│   └── generalAssembly.ts                   (GeneralAssemblyDto, PublishedGeneralAssembly)
└── services/
    └── urlservice.ts                        (Endpoint mappings)
```

---

## Configuration Reference

### MinIO Configuration (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "PostgresDb": "Host=localhost;Port=5432;...",
    "Minio": "localhost:9000"
  },
  "Minio": {
    "AccessKey": "ROOTUSER",
    "SecretKey": "CHANGEME123",
    "Secure": false,
    "PublicEndpoint": null  // Optional custom domain
  }
}
```

### Environment Variables

| Variable | Purpose |
|----------|---------|
| ConnectionStrings__Minio | MinIO public endpoint |
| ConnectionStrings__MinioInternal | MinIO internal cluster endpoint (optional) |
| Minio__AccessKey | S3 access key |
| Minio__SecretKey | S3 secret key |
| Minio__Secure | Enable TLS for MinIO |
| Minio__PublicEndpoint | Custom public domain (e.g., storage.example.com) |

---

## API Endpoint Reference

### Event Endpoints

```
GET    /events                  → List events (public)
GET    /events/{id}             → Get single event (public)
POST   /events                  → Create event [Admin]
PUT    /events/{id}             → Update event [Admin]
DELETE /events/{id}             → Delete event [Admin]
```

### News Endpoints

```
GET    /news                    → List news (public)
GET    /news/{id}               → Get single news (public)
POST   /news                    → Create news [Admin]
PUT    /news/{id}               → Update news [Admin]
DELETE /news/{id}               → Delete news [Admin]
```

### General Assembly Endpoints

```
GET    /general-assemblies              → List assemblies (public)
GET    /general-assemblies/{id}         → Get single assembly (public)
POST   /general-assemblies              → Create assembly [Admin]
PUT    /general-assemblies/{id}         → Update assembly [Admin]
DELETE /general-assemblies/{id}         → Delete assembly [Admin]
```

### File Storage Endpoints

```
GET    /bucket/events/{id}              → Get presigned URL for event thumbnail
GET    /bucket/news/{id}                → Get presigned URL for news thumbnail
GET    /bucket/general-assemblies/{id}  → Get presigned URL for GA thumbnail
POST   /imagecleanup                    → Clean orphaned images [Admin]
```

---

## Data Model Details

### Event Model

**DTO Fields:**
- Title (string, required)
- Section (string, required)
- Address (string, required)
- DateTime (string, required, ISO format)
- Description (string, required)
- ThumbnailId (string, optional GUID)

**Domain Fields (Additional):**
- ThumbnailUrl (string, constructed from ThumbnailId)
- CreatedAt (DateTime, UTC)

### News Model

**DTO Fields:**
- Title (string, required)
- Section (string, optional)
- Author (string, optional)
- Description (string, required)
- ThumbnailId (string, optional GUID)

**Domain Fields (Additional):**
- ThumbnailUrl (string, constructed from ThumbnailId)
- PublishedAt (DateTime, UTC)
- CreatedAt (DateTime, UTC)

### GeneralAssembly Model

**DTO Fields:**
- Title (string, required)
- Section (string, required)
- Address (string, required)
- DateTime (string, required, ISO format)
- Description (string, required)
- ThumbnailId (string, optional GUID)

**Domain Fields (Additional):**
- ThumbnailUrl (string, constructed from ThumbnailId)
- CreatedAt (DateTime, UTC)

---

## Validation & Security Layers

### Input Validation

**Backend:**
- [Required] attributes on mandatory fields
- GuidValidationAttribute on ThumbnailId (with AllowEmpty = true)
- DateTimeValidationAttribute on DateTime fields
- HtmlSanitizer (Ganss.Xss) on all string properties

**Frontend:**
- Required field validation (non-empty checks)
- Empty file rejection (size === 0)
- MIME type filtering (accept="image/*")

### File Security

- Presigned URLs signed with HMAC-SHA256
- 1-hour expiry on presigned URLs
- PUT-only scope (upload)
- Public-read bucket policy on buckets

**Gaps Identified:**
- No explicit file size limits
- No image dimension validation
- No MIME type validation on backend

---

## Design Patterns

### Repository Pattern
- Generic base: `IRepository<T, Y>` (T = domain, Y = DTO)
- 
