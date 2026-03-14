# Ansible Automation — dkef

Ansible playbooks for provisioning and managing the dkef application stack on a self-hosted
server. The automation installs all required tooling, creates a single-node
[KinD](https://kind.sigs.k8s.io/) (Kubernetes in Docker) cluster, and deploys PostgreSQL,
MinIO, the ASP.NET Core API, and the Vue frontend — all behind an nginx ingress with
Let's Encrypt TLS certificates.

---

## Control Machine Prerequisites

### 1. Install Ansible

```bash
sudo apt update && sudo apt install python3 pipx
pipx install --include-deps ansible
ansible --version   # confirm
```

### 2. Install required Ansible Galaxy collections

```bash
ansible-galaxy collection install -r requirements.yml
```

This installs `kubernetes.core >= 5.0.0`, which provides the `k8s`, `k8s_info`, `k8s_exec`,
`helm`, and `helm_repository` modules used throughout the playbooks.

### 3. Configure SSH access

Ansible connects to the target host over SSH. Use key-based authentication so that the
connection is fully automated (no password prompt):

```bash
ssh-copy-id anders@192.168.0.158
```

---

## WSL Users

Windows users typically run Ansible inside WSL. The recommended approach is to create an
`ansible.cfg` and a `.vault_pass` file outside the repository:

```bash
mkdir ~/dkef-ansible
echo -n 'your-vault-password' > ~/dkef-ansible/.vault_pass
```

**`~/dkef-ansible/ansible.cfg`:**
```ini
[defaults]
vault_password_file = ~/dkef-ansible/.vault_pass
```

Export the config path before running playbooks:

```bash
export ANSIBLE_CONFIG=~/dkef-ansible/ansible.cfg
```

This removes the need to pass `-J` on every command.

---

## Inventory

`inventory.ini` targets a single host:

| Host | User | Python |
|---|---|---|
| `192.168.0.158` | `anders` | `/usr/bin/python3` |

The host is a LAN server. Ports 80 and 443 are exposed externally via a WAF/router; ports
5432, 9000, and 9001 are accessible on the LAN only via KinD NodePort mappings.

---

## Secrets (Ansible Vault)

All secrets are stored in `vars/secrets.yaml`, encrypted with Ansible Vault (AES-256). The
file is never committed in plaintext.

### Creating the secrets file

Copy the example template and fill in every value:

```bash
cp vars/secrets.yaml.example vars/secrets.yaml
# edit vars/secrets.yaml with your actual values
ansible-vault encrypt vars/secrets.yaml
```

The following variables are required:

| Variable | Description |
|---|---|
| `postgres_user` | PostgreSQL superuser name |
| `postgres_password` | PostgreSQL superuser password |
| `postgres_db` | PostgreSQL database name |
| `minio_access_key` | MinIO root user (access key) |
| `minio_secret_key` | MinIO root password (secret key) |
| `api_postgres_connection_string` | Full Npgsql connection string (`Host=postgres;Port=5432;...`) |
| `api_minio_connection_string` | MinIO public hostname for presigned URLs (`storage.andersbjerregaard.com`) |
| `api_minio_internal_connection_string` | MinIO internal endpoint for server-side ops (`minio:9000`) |
| `jwt_key` | JWT signing key (minimum 32 characters) |

### Editing an encrypted file

```bash
ansible-vault edit vars/secrets.yaml
```

### Passing the vault password at runtime

Either use a `.vault_pass` file (see WSL section above), or pass `-J` to be prompted:

```bash
ansible-playbook -i inventory.ini site.yaml -K -J
```

---

## Roles Reference

| Role | Purpose | Depends On | Pinned Version |
|---|---|---|---|
| `core_utils` | Installs `jq` | — | — |
| `docker` | Installs Docker CE from the official APT repo | — | latest |
| `helm` | Installs Helm from the Buildkite APT repo | — | latest |
| `kind` | Installs the KinD binary | `docker` | v0.31.0 |
| `kubectl` | Installs the kubectl binary | — | v1.35.1 |
| `kubernetes_python` | Installs `python3-pip` + `kubernetes` pip package (required by `kubernetes.core`) | — | — |
| `dkef_cluster` | Creates/reconciles the KinD cluster and deploys nginx ingress | `docker`, `kind`, `kubectl`, `core_utils` | Kubernetes v1.35.1 |
| `k8_ns_init` | Stub — intended to initialise Kubernetes namespaces (TODO) | `dkef_cluster` | — |
| `cert_manager` | Installs cert-manager via Helm and applies the Let's Encrypt ClusterIssuer | `helm` | v1.20.0 |
| `postgres_k8s` | Creates the `postgres-secret` and deploys the PostgreSQL StatefulSet and Services | `dkef_cluster` | — |
| `minio_k8s` | Creates the `minio-secret` and deploys the MinIO StatefulSet, Services, and Ingress | `dkef_cluster` | — |
| `api_k8s` | Creates the API ConfigMap and Secret, resolves the Docker Hub image digest, and deploys the API | `postgres_k8s`, `minio_k8s` | — |
| `frontend_k8s` | Resolves the Docker Hub image digest and deploys the frontend | `postgres_k8s` | — |
| `postgres_backup` | Runs `pg_dumpall` inside the pod and copies the dump to the host | — | — |
| `postgres_restore` | Copies a dump from the host into the pod and runs `psql` to restore | — | — |
| `minio_backup` | Installs `mc` and mirrors all MinIO buckets to the host filesystem | — | RELEASE.2025-04-03... |
| `minio_restore` | Installs `mc` and mirrors the host backup back into MinIO | — | RELEASE.2025-04-03... |

### KinD cluster port mappings

The cluster is named `dkef` and is configured as a single control-plane node. The following
host ports are mapped into the KinD container:

| Host Port | Cluster Port | Purpose |
|---|---|---|
| 80 | 80 | HTTP / nginx ingress |
| 443 | 443 | HTTPS / nginx ingress |
| 5432 | 30432 (NodePort) | PostgreSQL (LAN access) |
| 9000 | 30900 (NodePort) | MinIO API (LAN access) |
| 9001 | 30901 (NodePort) | MinIO console (LAN access) |

If the cluster already exists but the port mappings have drifted from `kind-config.yaml`, the
`dkef_cluster` role will delete and recreate the cluster automatically.

---

## Playbooks

### `site.yaml` — Full provisioning

Provisions everything from scratch (or reconciles an existing deployment). Roles run in the
following order:

| Step | Role | Tag |
|---|---|---|
| 1 | `kubernetes_python` | `kubernetes_python` |
| 2 | `dkef_cluster` (+ `docker`, `kind`, `kubectl`, `core_utils` via meta) | `dkef_cluster` |
| 3 | `cert_manager` (+ `helm` via meta) | `cert_manager` |
| 4 | `postgres_k8s` | `postgres_k8s` |
| 5 | `minio_k8s` | `minio_k8s` |
| 6 | `api_k8s` | `api_k8s` |
| 7 | `frontend_k8s` | `frontend_k8s` |

`cert_manager` must run before the application roles because all Ingress resources carry the
`cert-manager.io/cluster-issuer: letsencrypt-prod` annotation — if the ClusterIssuer does not
exist, TLS provisioning will fail silently.

### `backup.yaml` — Backup

Backs up PostgreSQL (via `pg_dumpall`) and MinIO (via `mc mirror`) to the host filesystem.
Backups land in:

| Service | Host path |
|---|---|
| PostgreSQL | `/opt/dkef-backups/postgres/dkef-dump.sql` |
| MinIO | `/opt/dkef-backups/minio/` (mirrors all buckets) |

Both services are backed up by default. Use `--tags` to back up only one:

```bash
ansible-playbook -i inventory.ini backup.yaml -K --tags postgres
ansible-playbook -i inventory.ini backup.yaml -K --tags minio
```

### `restore.yaml` — Restore

Restores PostgreSQL and MinIO from the backup paths above. The playbook runs pre-flight checks
and fails fast if the expected backup files are not present on the host:

- `/opt/dkef-backups/postgres/dkef-dump.sql` must exist for Postgres restore.
- `/opt/dkef-backups/minio/` must exist for MinIO restore.

Use `--tags` to restore only one service:

```bash
ansible-playbook -i inventory.ini restore.yaml -K --tags postgres
ansible-playbook -i inventory.ini restore.yaml -K --tags minio
```

---

## Common Commands

All commands assume you are in the `automation/ansible/` directory.

**Full provisioning run (prompted for sudo and vault password):**
```bash
ansible-playbook -i inventory.ini site.yaml -K -J
```

**Full provisioning run (vault password from file):**
```bash
ansible-playbook -i inventory.ini site.yaml -K
```

**Re-deploy only the API (useful after a new Docker image is pushed):**
```bash
ansible-playbook -i inventory.ini site.yaml -K --tags api_k8s
```

**Re-deploy only the frontend:**
```bash
ansible-playbook -i inventory.ini site.yaml -K --tags frontend_k8s
```

**Re-run cluster setup (e.g. after changing `kind-config.yaml`):**
```bash
ansible-playbook -i inventory.ini site.yaml -K --tags dkef_cluster
```

**Back up everything:**
```bash
ansible-playbook -i inventory.ini backup.yaml -K
```

**Restore everything:**
```bash
ansible-playbook -i inventory.ini restore.yaml -K
```

**Encrypt or edit secrets:**
```bash
ansible-vault encrypt vars/secrets.yaml
ansible-vault edit vars/secrets.yaml
ansible-vault decrypt vars/secrets.yaml   # only for inspection — re-encrypt afterwards
```
