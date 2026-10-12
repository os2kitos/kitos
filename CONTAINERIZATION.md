# KITOS Containerization Guide

This document describes how to run KITOS in containers and how configuration, secrets, and certificates are managed.

## Running Locally with Podman Compose

```powershell
# Start full local stack with safe ordering
.\StartLocalDocker.ps1
```

This starts PostgreSQL first, prepares databases (role + owner-assigned databases), runs migrations, then starts API services and RabbitMQ.

### Script Parameters

| Parameter                     | Type   | Description                                                                                                                                       |
| ----------------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `-ResetData`                  | Switch | Complete reset: removes all containers and volumes, then recreates with fresh databases. Runs both database prep scripts.                         |
| `-RebuildApiImages`           | Switch | Rebuild `kitos-api` and `pubsub-api` Docker images before starting containers.                                                                    |
| `-NoCache`                    | Switch | When rebuilding images, skip Docker layer cache (use with `-RebuildApiImages`).                                                                   |
| `-KitosDbConnectionString`    | String | PostgreSQL connection string for KITOS database (default: `Host=localhost;Port=5432;Database=kitos;Username=kitos;Password=kitos`).               |
| `-HangfireDbConnectionString` | String | PostgreSQL connection string for Hangfire database (default: `Host=localhost;Port=5432;Database=kitos_hangfiredb;Username=kitos;Password=kitos`). |

### Examples

For a clean reset and rebuild:

```powershell
.\StartLocalDocker.ps1 -ResetData -RebuildApiImages -NoCache
```

Quick restart (keep databases intact):

```powershell
.\StartLocalDocker.ps1
```

Skip PubSub database prep:

```powershell
.\StartLocalDocker.ps1 -SkipPubSubDatabasePrepare
```

### Using Podman Compose Directly

For quick restarts without the PowerShell script overhead, use `podman compose` directly:

```powershell
# Start/restart all services (assumes databases already exist and schema is already up to date)
podman compose up -d

# Stop all services
podman compose down

# View logs
podman compose logs -f kitos-api
```

The script (`StartLocalDocker.ps1`) is recommended for initial setup and clean resets, but `podman compose` commands are faster for day-to-day development when you just need to restart containers.

### Services

| Service    | Port         | Description                             |
| ---------- | ------------ | --------------------------------------- |
| kitos-api  | 5000         | Main KITOS API + legacy AngularJS UI    |
| pubsub-api | 5100         | PubSub event service                    |
| postgres   | 5432         | PostgreSQL database                     |
| rabbitmq   | 5672 / 15672 | Message broker (management UI on 15672) |

### What `.env` Needs to Contain

See `.env.example` for the full template. At minimum:

```env
SSO_CERT_FILE_NAME=kitos-local-pfx
STS_CERT_FILE_NAME=ADG_EXTTEST_Adgangsstyring_2
STS_ORG_CERT_FILE_NAME=ORG_EXTTEST_Organisation_2
SSO_CERT_PASSWORD=<password for kitos-local-pfx>
STS_CERT_PASSWORD=<password for ADG_EXTTEST_Adgangsstyring_2>
STS_ORG_CERT_PASSWORD=<password for ORG_EXTTEST_Organisation_2>
SSO_CERT_THUMBPRINT=<thumbprint for SSO certificate>
STS_CERT_THUMBPRINT=<thumbprint for STS certificate>
STS_ORG_CERT_THUMBPRINT=<thumbprint for STS organisation certificate>
```

## Configuration

KITOS uses ASP.NET Core's layered configuration system:

1. **`appsettings.json`** — base configuration baked into the image
2. **Environment variables** — override any setting using `__` as section separator  
   Example: `AppSettings__BaseUrl` overrides `AppSettings.BaseUrl`
3. **`appsettings.{ENVIRONMENT}.json`** — per-environment overrides (optional)

## Certificates

KITOS uses X.509 certificates for communication with Danish government services (KOMBIT STS, Serviceplatformen).

### How It Works

The `CertificateLoader` class supports two modes:

1. **PFX file-based** (containers/Linux) — set `*CertFilePath` config keys to mount PFX files
2. **Windows Certificate Store** (legacy/IIS) — leave `*CertFilePath` empty to use thumbprint-based store lookup

The system automatically falls back: if a `*CertFilePath` is set, the PFX file is loaded and validated against the configured thumbprint. If not, the Windows certificate store is used with the configured thumbprint.

### Certificate Config Keys

| Purpose              | File Path Key                             | Password Key                              | Thumbprint Key (fallback)                          |
| -------------------- | ----------------------------------------- | ----------------------------------------- | -------------------------------------------------- |
| SSO Service Provider | `AppSettings:SsoCertFilePath`             | `AppSettings:SsoCertPassword`             | `AppSettings:SsoCertificateThumbprint`             |
| STS (Adgangsstyring) | `AppSettings:StsCertFilePath`             | `AppSettings:StsCertPassword`             | `AppSettings:StsCertificateThumbprint`             |
| STS Organisation     | `AppSettings:StsOrganisationCertFilePath` | `AppSettings:StsOrganisationCertPassword` | `AppSettings:StsOrganisationCertificateThumbprint` |

### STS Certificate Validation in Containers

To replicate production behavior locally, keep strict STS certificate checks enabled:

- `AppSettings__StsCertificateValidationMode=ChainTrust`
- `AppSettings__StsCertificateRevocationMode=Online`

In Podman Compose these are mapped from:

- `STS_CERT_VALIDATION_MODE` (default `ChainTrust`)
- `STS_CERT_REVOCATION_MODE` (default `Online`)

Only for non-production troubleshooting, you can temporarily relax checks to values like:

- `AppSettings__StsCertificateValidationMode` (for example `None`)
- `AppSettings__StsCertificateRevocationMode` (for example `NoCheck`)

### Podman Compose Cert Setup

Place PFX files in the `./certs/` directory at the repo root. They are mounted read-only into the container at `/etc/ssl/certs/`:

```yaml
volumes:
  - ./certs/kitos-local-pfx:/etc/ssl/certs/kitos-local-pfx:ro
  - ./certs/ADG_EXTTEST_Adgangsstyring_2.pfx:/etc/ssl/certs/ADG_EXTTEST_Adgangsstyring_2.pfx:ro
  - ./certs/ORG_EXTTEST_Organisation_2.pfx:/etc/ssl/certs/ORG_EXTTEST_Organisation_2.pfx:ro
```

To replicate production trust behavior in Podman, also place trust-chain certificates
(`.crt` in PEM format) in `./certs/` (for example root/intermediate CA). The `kitos-api`
container imports `./certs/*.crt` into `/usr/local/share/ca-certificates/` and runs
`update-ca-certificates` on startup before launching the API.

## Database Migrations

For local development, run `DeploymentScripts/PrepareLocalDatabase.ps1` against the running PostgreSQL container to prepare the KITOS and Hangfire databases.

```powershell
.\DeploymentScripts\PrepareLocalDatabase.ps1 `
  -kitosDbConnectionString "Host=localhost;Port=5432;Database=kitos;Username=kitos;Password=kitos" `
  -hangfireDbConnectionString "Host=localhost;Port=5432;Database=kitos_hangfiredb;Username=kitos;Password=kitos"
```

For PubSub, run:

```powershell
.\DeploymentScripts\PrepareLocalPubSubDatabase.Postgres.ps1
```

Or run everything in one step:

```powershell
.\StartLocalDocker.ps1
```
