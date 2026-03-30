# Backend Codebase Exploration Report
## dkef-api File Storage, DTOs, Controllers & Database Structure

**Date:** March 2026  
**Project:** dkef (DKEF Member Management System)  
**Focus:** Backend file storage, entity DTOs, controllers, and database architecture

---

## 1. File Storage Architecture

### MinIO/S3 Integration

**File Storage:** MinIO (S3-compatible object storage) with 3 buckets:
- `events` — Event thumbnail images
- `news` — News article thumbnail images
- `general-assemblies` — General assembly thumbnail images

**Service:** `MinioBucketService` (implements `IBucketService`)
**Location:** `src/dkef-api/Services/MinioBucketService.cs`

#### Presigned URLs
- Generated via `GetPresignedUrlAsync(bucket, objectName, isPublic = false)`
- Expiry: 3600 seconds (1 hour)
- Returns PUT-only URLs for client-side uploads
- Supports HTTPS rewriting for TLS-terminating proxies
- Bucket is set to public-read on first upload

#### Configuration
```
ConnectionStrings:Minio = MinIO public endpoint
ConnectionStrings:MinioInternal = Optional internal cluster endpoint
Minio:PublicEndpoint = Custom domain (e.g., storage.example.com)
Minio:AccessKey / SecretKey = S3 credentials
Minio:Secure = SSL/TLS flag
```

#### BucketController Endpoints
- `GET /bucket/events/{id}` → Presigned URL for event thumbnail
- `GET /bucket/news/{id}` → Presigned URL for news thumbnail
- `GET /bucket/general-assemblies/{id}` → Presigned URL for GA thumbnail

#### Image Cleanup
- Service: `ImageCleanupService`
- Scans all buckets for orphaned images (in MinIO but not referenced by DB)
- Deletes unreferenced files
- Triggered by admin endpoint

---

## 2. DTOs & Domain Models

### Event

**DTO Properties:**
- Title (required)
- Section (required)
- Address (required)
- DateTime (required, validated)
- Description (required)
- ThumbnailId (optional GUID)

**Domain Properties:**
- All above + ThumbnailUrl (string, constructed from ThumbnailId)
- CreatedAt (timestamp, set on creation)

**Validation:**
- HTML sanitized via HtmlSanitizer
- DateTime validated with custom attribute
- ThumbnailId validated as GUID (optional)

### News

**DTO Properties:**
- Title (required)
- Section (optional)
- Author (optional)
- Description (required)
- ThumbnailId (optional GUID)

**Domain Properties:**
- All above + ThumbnailUrl (string)
- PublishedAt (timestamp, set on creation)
- CreatedAt (timestamp, set on creation)

### GeneralAssembly

**DTO Properties:**
- Title (required)
- Section (required)
- Address (required)
- DateTime (required, validated)
- Description (required)
- ThumbnailId (optional GUID)

**Domain Properties:**
- All above + ThumbnailUrl (string)
- CreatedAt (timestamp, set on creation)

**Common Pattern:**
- ThumbnailId (GUID) in DTO → ThumbnailUrl (full URL) in domain via AutoMapper
- Example URL: `http://localhost:9000/events/{thumbnailId}`

---

## 3. API Controllers

### EventsController (`/events`)

**Endpoints:**
- GET /events — List (pagination: take, skip; sorting: orderBy, order)
- GET /events/{id} — Get single
- POST /events — Create (Admin only)
- PUT /events/{id} — Update (Admin only)
- DELETE /events/{id} — Delete (Admin only)

**Features:**
- Max 50 items per page
- Sortable: DateTime, CreatedAt
- HTML sanitization on POST/PUT
- Fire-and-forget image cleanup on DELETE

### NewsController (`/news`)

**Same pattern as EventsController:**
- Full CRUD with Admin role requirement
- Supports PublishedAt in domain model
- Optional Section/Author fields

### GeneralAssembliesController (`/general-assemblies`)

**Same pattern as EventsController**
- DateTime field required
- Address field required

---

## 4. Database Structure

### DbContexts (Independent)

| Context | Table | Location |
|---------|-------|----------|
| EventsContext | Events | src/dkef-api/Data/EventsContext.cs |
| NewsContext | News | src/dkef-api/Data/NewsContext.cs |
| GeneralAssemblyContext | GeneralAssemblies | src/dkef-api/Data/GeneralAssemblyContext.cs |

### Migrations

**Events Table (20260309194134_EventsInitialCreate):**
```
Id (UUID, PK)
Title (text, NOT NULL)
Section (text, NOT NULL)
Address (text, NOT NULL)
DateTime (timestamp with time zone, NOT NULL)
Description (text, NOT NULL)
ThumbnailUrl (text, NOT NULL)
CreatedAt (timestamp with time zone, NOT NULL)
```

**News Table (20260309194136_NewsInitialCreate):**
```
Id (UUID, PK)
Title (text, NOT NULL)
Section (text, NOT NULL)
Author (text, NOT NULL)
Description (text, NOT NULL)
ThumbnailUrl (text, NOT NULL)
PublishedAt (timestamp with time zone, NOT NULL)
CreatedAt (timestamp with time zone, NOT NULL)
```

**GeneralAssemblies Table (20260309194134_GeneralAssemblyInitialCreate):**
```
Id (UUID, PK)
Title (text, NOT NULL)
Section (text, NOT NULL)
Address (text, NOT NULL)
DateTime (timestamp with time zone, NOT NULL)
Description (text, NOT NULL)
ThumbnailUrl (text, NOT NULL)
CreatedAt (timestamp with time zone, NOT NULL)
```

### Database Provider
- **PostgreSQL** 14+
- **Automatic migrations** on application startup

---

## 5. Frontend-Backend Integration

### File Upload Workflow

1. **Generate UUID** in frontend
2. **Request presigned URL:**
   ```
   GET /bucket/events/{uuid}
   ```
3. **Upload to MinIO directly:**
   ```
   PUT {presignedUrl} with file
   ```
4. **Save entity with ThumbnailId:**
   ```
   POST/PUT /events with dto.thumbnailId = uuid
   ```

### Modal Components

**EditEventModal.vue:**
- File input with `accept="image/*"`
- Empty file validation (size === 0)
- Presigned URL request
- Direct MinIO upload
- Entity update with thumbnailId

**EditNewsModal.vue:**
- Same pattern as Event
- Optional fields (Section, Author)

**EditGeneralAssemblyModal.vue:**
- Same pattern as Event
- Required Address & DateTime

### URL Service

Maps operations to endpoints:
```typescript
getEventPresignedUrl(guid) → /bucket/events/{guid}
getNewsPresignedUrl(guid) → /bucket/news/{guid}
getGeneralAssemblyPresignedUrl(guid) → /bucket/general-assemblies/{guid}
```

---

## 6. File Validation & Security

### Input Validation (Backend)

**GuidValidationAttribute:**
- Validates ThumbnailId is valid GUID
- AllowEmpty = true for optional thumbnails

**DateTimeValidationAttribute:**
- Validates DateTime field format
- Uses invariant culture parsing

**Required Attributes:**
- Title, DateTime, Description are [Required(AllowEmptyStrings = false)]
- ThumbnailId has [GuidValidation(AllowEmpty = true)]

### HTML Sanitization

**Library:** Ganss.Xss.HtmlSanitizer

**Applied to:** Title, Section, Address, Description (all string fields)

**Execution:** Called on all DTOs before mapping to domain

### Frontend Validation

**EditModalComponents:**
- Empty field validation (trim check)
- Empty file rejection (size === 0)
- File type filtering (accept="image/*")
- No max file size validation currently

### Presigned URL Security

- HMAC-SHA256 signed URLs
- 1-hour expiry
- PUT-only scope (upload)
- Public-read bucket policy

**Missing:** Explicit file size limits (recommend adding)

---

## 7. Key Implementation Patterns

### Repository Pattern

**Base Interface:** `IRepository<T, Y>` (generic domain/DTO pattern)
```
GetByIdAsync(Guid id)
GetMultipleAsync(take, skip)
GetMultipleAsync(IOrderedQueryable, take, skip)
CreateAsync(T dto)
UpdateAsync(Guid id, Y dto)
DeleteAsync(Guid id)
```

**Implementations:**
- EventsRepository : IEventsRepository
- NewsRepository : INewsRepository
- GeneralAssemblyRepository : IGeneralAssemblyRepository

### Image Lifecycle

**On Update:**
- Compares old vs new ThumbnailId
- Deletes old image from MinIO if changed (fire-and-forget)
- Update proceeds regardless of cleanup result

**On Delete:**
- Extracts GUID from ThumbnailUrl
- Triggers async delete (non-blocking)
- Returns success even if cleanup fails

**Pattern:** File operations never block entity operations

### Pagination

**Return Type:** `DomainCollection<T>`
```csharp
public class DomainCollection<T>
{
    public IEnumerable<T> Collection { get; }
    public int Total { get; }
}
```

### Sorting

**Sortable Attribute:** Applied to DateTime, CreatedAt, PublishedAt

**QueryableService<T
