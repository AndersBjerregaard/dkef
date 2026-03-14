# dkef — Docker Build Guide

This directory contains the two application Dockerfiles and the Compose file used for local development.

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

## Local development (single architecture)

Start all services (API, frontend, PostgreSQL, pgAdmin, MinIO) with Docker Compose:

```sh
# from src/
docker compose up
```

| Service | URL |
|---|---|
| Frontend | http://localhost:5173 |
| API | http://localhost:5275 |
| pgAdmin | http://localhost:8080 |
| MinIO console | http://localhost:9001 |
| MinIO S3 endpoint | http://localhost:9000 |

To rebuild images before starting:

```sh
docker compose up --build
```

---

## Multi-architecture builds (`linux/amd64` + `linux/arm64`)

Cross-platform images are built with **Docker Buildx** and **QEMU**.

### 1. Install QEMU (one-time setup)

```sh
docker run --privileged --rm tonistiigi/binfmt --install all
```

### 2. Create (or reuse) a Buildx builder

```sh
docker buildx create --name multiarch-builder --driver docker-container --use
docker buildx inspect --bootstrap
```

### 3. Build and push `dkef-api`

```sh
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  --tag <your-dockerhub-username>/dkef-api:latest \
  --push \
  ./dkef-api
```

### 4. Build and push `dkef-frontend`

```sh
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  --tag <your-dockerhub-username>/dkef-frontend:latest \
  --push \
  ./dkef-vue
```

> **Note:** `--push` uploads the multi-arch manifest directly to the registry.  
> To inspect the result locally without pushing, replace `--push` with `--load` (single platform only) or `--output type=oci,dest=image.tar`.

### 5. Verify the manifest

After pushing, confirm both architectures are present:

```sh
docker buildx imagetools inspect <your-dockerhub-username>/dkef-api:latest
docker buildx imagetools inspect <your-dockerhub-username>/dkef-frontend:latest
```

---

## CI/CD (GitHub Actions)

Multi-arch builds are automated via `.github/workflows/build.yml` and triggered manually (`workflow_dispatch`).  
The workflow uses `docker/setup-qemu-action` and `docker/setup-buildx-action`, then builds and pushes both images to Docker Hub for `linux/amd64` and `linux/arm64`.

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