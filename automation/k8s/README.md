# Kubernetes Manifests — dkef

Raw Kubernetes manifests for the dkef application stack. These files are **not applied
directly** — they are applied by the [Ansible playbooks](../ansible/README.md), which inject
secrets at runtime and override image tags with immutable SHA digests before applying.

---

## Directory Structure

```
k8s/
├── cert-manager/
│   └── cluster-issuer.yaml       # Let's Encrypt production ClusterIssuer
├── postgres/
│   ├── statefulset.yaml          # PostgreSQL StatefulSet
│   ├── service.yaml              # ClusterIP Service (cluster-internal access)
│   └── nodeport-service.yaml     # NodePort Service (LAN access via host port 5432)
├── minio/
│   ├── statefulset.yaml          # MinIO StatefulSet
│   ├── service.yaml              # ClusterIP Service (ports 9000 + 9001)
│   ├── service-nodeport.yaml     # NodePort Service (LAN access via host ports 9000/9001)
│   └── ingress.yaml              # Ingress: storage.andersbjerregaard.com (TLS + CORS)
├── api/
│   ├── configmap.yaml            # Non-secret runtime configuration
│   ├── deployment.yaml           # ASP.NET Core API Deployment
│   ├── service.yaml              # ClusterIP Service (port 80 → container 8080)
│   └── ingress.yaml              # Ingress: api.andersbjerregaard.com (TLS)
└── frontend/
    ├── deployment.yaml           # Vue SPA / nginx Deployment
    ├── service.yaml              # ClusterIP Service (port 80 → container 80)
    └── ingress.yaml              # Ingress: andersbjerregaard.com (TLS)
```

All resources are deployed into the `default` namespace unless noted otherwise.

---

## `cert-manager/`

### `cluster-issuer.yaml`

| Field | Value |
|---|---|
| Kind | `ClusterIssuer` (cluster-scoped, namespace: n/a) |
| Name | `letsencrypt-prod` |
| ACME server | Let's Encrypt v2 production |
| Challenge solver | HTTP-01 via nginx IngressClass |
| Account key Secret | `letsencrypt-prod-account-key` (namespace: `cert-manager`) |

All three Ingress resources (API, frontend, MinIO) carry the annotation
`cert-manager.io/cluster-issuer: letsencrypt-prod`, which causes cert-manager to
automatically provision and renew TLS certificates via Let's Encrypt.

Applied by the `cert_manager` Ansible role. Must exist before any Ingress resources are
created, or TLS provisioning will fail.

---

## `postgres/`

### `statefulset.yaml`

| Field | Value |
|---|---|
| Name | `postgres` |
| Image | `postgres:18.3` |
| Replicas | 1 |
| PVC size | 5Gi (`ReadWriteOnce`) |
| Data directory | `/var/lib/postgresql/data/pgdata` |
| `/dev/shm` | 128Mi in-memory `emptyDir` (improves sort/hash performance) |
| Liveness probe | `pg_isready` |
| Readiness probe | `pg_isready` |
| Secret consumed | `postgres-secret` |

### `service.yaml` — ClusterIP

| Field | Value |
|---|---|
| Name | `postgres` |
| Port | 5432 → 5432 |
| Purpose | Cluster-internal access; the API connects via `Host=postgres` in its connection string |

### `nodeport-service.yaml` — NodePort

| Field | Value |
|---|---|
| Name | `postgres-nodeport` |
| NodePort | 30432 → 5432 |
| Host port | 5432 (mapped by KinD) |
| Purpose | LAN-only access — the WAF does not expose port 5432 externally |

### Secret: `postgres-secret`

Created by the `postgres_k8s` Ansible role. Not present in these manifests.

| Key | Purpose |
|---|---|
| `postgres_user` | Superuser name |
| `postgres_password` | Superuser password |
| `postgres_db` | Database name |

---

## `minio/`

### `statefulset.yaml`

| Field | Value |
|---|---|
| Name | `minio` |
| Image | `quay.io/minio/minio:RELEASE.2025-09-07T16-13-09Z` |
| Replicas | 1 |
| PVC size | 10Gi (`ReadWriteOnce`) |
| API port | 9000 |
| Console port | 9001 |
| Liveness probe | `GET /minio/health/live:9000` |
| Readiness probe | `GET /minio/health/ready:9000` |
| Secret consumed | `minio-secret` |

### `service.yaml` — ClusterIP

| Field | Value |
|---|---|
| Name | `minio` |
| Ports | 9000 (API), 9001 (console) |
| Purpose | Cluster-internal access; the API connects via `minio:9000` |

### `service-nodeport.yaml` — NodePort

| Field | Value |
|---|---|
| Name | `minio-nodeport` |
| NodePorts | 30900 → 9000, 30901 → 9001 |
| Host ports | 9000 → 9000, 9001 → 9001 (mapped by KinD) |
| Purpose | LAN-only access for direct bucket management |

### `ingress.yaml`

| Field | Value |
|---|---|
| Host | `storage.andersbjerregaard.com` |
| TLS Secret | `minio-tls` (auto-created by cert-manager) |
| Backend | `minio:9000` |
| CORS | Enabled for `https://andersbjerregaard.com` — `GET`, `PUT`, `OPTIONS`; all headers |
| Purpose | Public endpoint for client-side presigned URL uploads from the frontend |

### Secret: `minio-secret`

Created by the `minio_k8s` Ansible role. Not present in these manifests.

| Key | Purpose |
|---|---|
| `minio_access_key` | MinIO root user |
| `minio_secret_key` | MinIO root password |

---

## `api/`

### `configmap.yaml`

**Name:** `dkef-api-config`

Contains non-secret environment variables for the ASP.NET Core application:

| Key | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://*:8080` |
| `Minio__Secure` | `false` (TLS is terminated at the ingress) |
| `Minio__PublicEndpoint` | `storage.andersbjerregaard.com` |
| `JwtSettings__Issuer` | JWT issuer claim |
| `JwtSettings__Audience` | JWT audience claim |
| `JwtSettings__ExpiryMinutes` | Access token lifetime |
| `JwtSettings__RefreshTokenExpiryDays` | Refresh token lifetime |
| Log level overrides | `Information` default, `Warning` for ASP.NET Core and EF Core |

Applied by the `api_k8s` Ansible role from this file.

### `deployment.yaml`

| Field | Value |
|---|---|
| Name | `dkef-api` |
| Image | `anders97/dkef-api:latest` (**overridden** to pinned SHA digest by Ansible at deploy time) |
| Replicas | 1 |
| Container port | 8080 |
| Liveness probe | `GET /health:8080` |
| Readiness probe | `GET /health:8080` |
| ConfigMap consumed | `dkef-api-config` |
| Secret consumed | `dkef-api-secret` |

### `service.yaml` — ClusterIP

| Field | Value |
|---|---|
| Name | `dkef-api` |
| Port | 80 → 8080 |

### `ingress.yaml`

| Field | Value |
|---|---|
| Host | `api.andersbjerregaard.com` |
| TLS Secret | `dkef-api-tls` (auto-created by cert-manager) |
| Backend | `dkef-api:80` |

### Secret: `dkef-api-secret`

Created by the `api_k8s` Ansible role. Not present in these manifests.

| Key | Purpose |
|---|---|
| `ConnectionStrings__PostgresDb` | Full Npgsql connection string |
| `ConnectionStrings__Minio` | MinIO public hostname for presigned URL generation |
| `ConnectionStrings__MinioInternal` | MinIO internal endpoint for server-side operations |
| `Minio__AccessKey` | MinIO root user |
| `Minio__SecretKey` | MinIO root password |
| `JwtSettings__Key` | JWT signing key (minimum 32 characters) |

---

## `frontend/`

### `deployment.yaml`

| Field | Value |
|---|---|
| Name | `dkef-frontend` |
| Image | `anders97/dkef-frontend:latest` (**overridden** to pinned SHA digest by Ansible at deploy time) |
| Replicas | 1 |
| Container port | 80 (nginx) |

The frontend is a pre-built static Vue SPA served by nginx. All runtime configuration
(API base URL, mode) is baked in at Docker image build time via `VITE_` environment
variables — there are no environment variables or secrets injected at runtime.

### `service.yaml` — ClusterIP

| Field | Value |
|---|---|
| Name | `dkef-frontend` |
| Port | 80 → 80 |

### `ingress.yaml`

| Field | Value |
|---|---|
| Host | `andersbjerregaard.com` |
| TLS Secret | `dkef-frontend-tls` (auto-created by cert-manager) |
| Backend | `dkef-frontend:80` |

---

## Image Pinning

The `deployment.yaml` files for `dkef-api` and `dkef-frontend` reference the `:latest` tag.
**At deploy time**, the `api_k8s` and `frontend_k8s` Ansible roles query the Docker Hub API to
resolve the current `latest` tag to its immutable SHA-256 digest and override the image
reference before applying:

```
anders97/dkef-api@sha256:<digest>
anders97/dkef-frontend@sha256:<digest>
```

This means every deployment is pinned to a specific image layer, even though the manifest
itself does not encode the digest. The actual running digest can be inspected with:

```bash
kubectl get pod -l app=dkef-api -o jsonpath='{.items[0].spec.containers[0].image}'
```

---

## Networking Summary

| Domain | Service | Port | TLS | External |
|---|---|---|---|---|
| `andersbjerregaard.com` | `dkef-frontend` | 80 | Yes (Let's Encrypt) | Yes |
| `api.andersbjerregaard.com` | `dkef-api` | 80 | Yes (Let's Encrypt) | Yes |
| `storage.andersbjerregaard.com` | `minio` | 9000 | Yes (Let's Encrypt) | Yes |
| `postgres` (cluster DNS) | `postgres` | 5432 | No | No |
| `minio` (cluster DNS) | `minio` | 9000 / 9001 | No | No |
| Host port 5432 (LAN) | `postgres-nodeport` | 30432 | No | No |
| Host port 9000 (LAN) | `minio-nodeport` | 30900 | No | No |
| Host port 9001 (LAN) | `minio-nodeport` | 30901 | No | No |

All external HTTPS traffic enters through the nginx ingress controller on the KinD
control-plane node (host ports 80/443). The WAF/router forwards only ports 80 and 443
from the internet to the server — database and MinIO management ports are LAN-only.
