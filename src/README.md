# dkef — Docker Build Guide

This directory contains the two application Dockerfiles.

| Image | Context | Dockerfile |
|---|---|---|
| `dkef-api` | `src/dkef-api/` | `dkef-api/Dockerfile` |
| `dkef-frontend` | `src/dkef-vue/` | `dkef-vue/Dockerfile` |

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine on Linux) with **Buildx** support
- QEMU user-space emulation for cross-architecture builds (see below)

Verify Buildx is available:

```sh
docker buildx version
```

---

## Docker image builds (`linux/amd64`)

Production images are built for `linux/amd64`.

### 1. Create (or reuse) a Buildx builder

```sh
docker buildx create --name amd64-builder --driver docker-container --use
docker buildx inspect --bootstrap
```

### 2. Build and push `dkef-api`

```sh
docker buildx build \
  --platform linux/amd64 \
  --tag <your-dockerhub-username>/dkef-api:<version> \
  --tag <your-dockerhub-username>/dkef-api:latest \
  --push \
  ./dkef-api
```

### 3. Build and push `dkef-frontend`

```sh
docker buildx build \
  --platform linux/amd64 \
  --tag <your-dockerhub-username>/dkef-frontend:<version> \
  --tag <your-dockerhub-username>/dkef-frontend:latest \
  --push \
  ./dkef-vue
```

> **Note:** `--push` uploads the image directly to the registry.  
> To inspect the result locally without pushing, replace `--push` with `--load` (single platform only) or `--output type=oci,dest=image.tar`.

### 4. Verify the published image

After pushing, inspect the tag metadata:

```sh
docker buildx imagetools inspect <your-dockerhub-username>/dkef-api:latest
docker buildx imagetools inspect <your-dockerhub-username>/dkef-frontend:latest
```

---

## CI/CD (GitHub Actions)

Builds are automated via `.github/workflows/build.yml` and support both tag-based releases and manual dispatch.

- **Architecture:** `linux/amd64` only
- **Independent versioning:** backend and frontend are released separately
- **Release tags:**
  - API: `api-vMAJOR.MINOR.PATCH` (example: `api-v0.17.0`)
  - Frontend: `frontend-vMAJOR.MINOR.PATCH` (example: `frontend-v0.17.0`)

Each release publishes this tag structure for the relevant image:

- `<MAJOR>.<MINOR>.<PATCH>` (example: `0.17.0`)
- `<MAJOR>.<MINOR>` (example: `0.17`)
- `<MAJOR>` (example: `0`)
- `latest`
- `sha-<shortsha>`

`workflow_dispatch` can also build `api`, `frontend`, or `both` by providing semver values in the workflow inputs.

Required repository secrets / variables:

| Name | Type | Description |
|---|---|---|
| `DOCKERHUB_USERNAME` | Variable | Docker Hub username |
| `DOCKERHUB_TOKEN` | Secret | Docker Hub access token |

---

## Environment variables

### API (`dkef-api/.env`)

| Variable | Description |
|---|---|
| `ConnectionStrings__PostgresDb` | PostgreSQL connection string |
| `ConnectionStrings__Minio` | MinIO endpoint |
| `Minio__AccessKey` | MinIO access key |
| `Minio__SecretKey` | MinIO secret key |
| `Minio__Secure` | `true` / `false` |
| `JwtSettings__Key` | JWT signing key |
| `JwtSettings__Issuer` | JWT issuer |
| `JwtSettings__Audience` | JWT audience |
| `JwtSettings__ExpiryMinutes` | Token lifetime in minutes |

### Frontend (build-time, baked into the image)

| Variable | Default (Docker build) | Description |
|---|---|---|
| `VITE_API_BASE_URL` | `http://localhost:5275` | Backend API base URL |
| `VITE_MODE` | `Docker` | Runtime mode label |
