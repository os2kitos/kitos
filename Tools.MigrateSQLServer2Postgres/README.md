# Tools.MigrateSQLServer2Postgres

Console tool for copy-over migration from SQL Server to PostgreSQL.

The design goal is safety: SQL Server remains authoritative while PostgreSQL is treated as a disposable target until validation passes.

The CLI supports both scripted execution and an interactive Spectre.Console wizard with arrow-key navigation.

1. Connection checks (SQL Server + PostgreSQL)
2. Target readiness handling (create/recreate target when needed)
3. Readiness checks (schema overlap and target emptiness guard)
4. Data migration
5. Validation

Fresh PostgreSQL targets are bootstrapped by running the EF Core migration chain from an empty database.
`InitialBaseline` now carries baseline schema creation directly, so there is no separate baseline SQL bootstrap step and no migration pre-marking.
When a PostgreSQL migrations bundle is available, the tool will only use it if it is newer than the migration source inputs; otherwise it falls back to `dotnet ef` from the current repository so newly-added migrations are not missed silently.

Interactive mode can:

* Reuse the last saved interactive settings
* Start from a local development preset for SQL Server Express and local PostgreSQL
* Prompt with editable defaults before execution
* Choose migration strategy before copy stage: start fresh or use current settings
* Confirm overwriting an existing target database before recreation

## Usage

The tool supports two subcommands: the default KITOS database migration, and a dedicated `pubsub` subcommand for the PubSub service database.

### KITOS database (default)

Interactive mode:

```powershell
dotnet run --project Tools.MigrateSQLServer2Postgres/Tools.MigrateSQLServer2Postgres.csproj -- --interactive
```

Scripted single-flow run:

```powershell
dotnet run --project Tools.MigrateSQLServer2Postgres/Tools.MigrateSQLServer2Postgres.csproj -- --source "Server=.\\SQLEXPRESS;Database=Kitos;Integrated Security=true;TrustServerCertificate=true" --target "Host=127.0.0.1;Port=5432;Database=kitos;Username=postgres;Password=<password>"
```

### PubSub database

Interactive mode:

```powershell
dotnet run --project Tools.MigrateSQLServer2Postgres/Tools.MigrateSQLServer2Postgres.csproj -- pubsub --interactive
```

Scripted single-flow run:

```powershell
dotnet run --project Tools.MigrateSQLServer2Postgres/Tools.MigrateSQLServer2Postgres.csproj -- pubsub --source "Server=.\\SQLEXPRESS;Database=KitosPubSub;Integrated Security=true;TrustServerCertificate=true" --target "Host=127.0.0.1;Port=5432;Database=kitos_pubsub;Username=postgres;Password=<password>"
```

The `pubsub` subcommand migrates the `Subscriptions` table from the PubSub SQL Server database to PostgreSQL.
The target schema is bootstrapped using EF Core migrations (`PubSubContext.Database.MigrateAsync()`), keeping it aligned with the PubSub application's own migration history.
## Local PostgreSQL development database

For local development you can run PostgreSQL in a Docker-compatible container runtime such as Podman or Docker.
The example below exposes PostgreSQL on the default local port `5432` and mounts a named volume so database files survive container restarts and container recreation.

Create and start the container with Podman:

```powershell
podman run --name kitos-postgres -d `
  -e POSTGRES\_USER=postgres `
  -e POSTGRES\_PASSWORD=postgres `
  -e POSTGRES\_DB=kitos `
  -v kitos\_pgdata:/var/lib/postgresql/data `
  -p 5432:5432 `
  postgres:17
```

The equivalent Docker command is:

```powershell
docker run --name kitos-postgres -d `
  -e POSTGRES\_USER=postgres `
  -e POSTGRES\_PASSWORD=postgres `
  -e POSTGRES\_DB=kitos `
  -v kitos\_pgdata:/var/lib/postgresql/data `
  -p 5432:5432 `
  postgres:17
```

Connection string for the migration tool:

```text
Host=127.0.0.1;Port=5432;Database=kitos;Username=postgres;Password=postgres
```

Verify that the container is running:

```powershell
podman ps
```

Open a PostgreSQL shell inside the container:

```powershell
podman exec -it kitos-postgres psql -U postgres -d kitos
```

Stop and start the container without deleting data:

```powershell
podman stop kitos-postgres
podman start kitos-postgres
```

Remove the container while keeping the database volume:

```powershell
podman stop kitos-postgres
podman rm kitos-postgres
```

Recreate the container with the same `kitos\_pgdata` volume to reuse the existing database files.

To intentionally delete the local database data, remove the volume after stopping and removing the container:

```powershell
podman volume rm kitos\_pgdata
```

Important: `POSTGRES\_USER`, `POSTGRES\_PASSWORD`, and `POSTGRES\_DB` are only applied when PostgreSQL initializes an empty data directory. If the `kitos\_pgdata` volume already contains a database, changing those environment variables will not reset the existing user, password, or database.

## Flags

* `--allow-non-empty-target`: allows running against a non-empty PostgreSQL target
* `--continue-on-error`: keeps migrating remaining tables after a table failure during the data copy stage
* `--interactive`: launches an interactive wizard with arrow-key navigation

Interactive settings are stored under the current user's application data folder so the next interactive run can reuse them.

## Migration journal

The data copy stage creates and updates:

* `public.\_\_kitos\_migration\_journal`

This table stores per-table status (`Started`, `Succeeded`, `Failed`), timestamps, copied row counts, and error text.

## Important behaviour

* Source SQL Server data is never modified by this tool.
* Tables are copied in topological order derived from both SQL Server and PostgreSQL foreign key edges, so parent tables are always inserted before their children.
* Foreign key checks are always disabled for the duration of the data copy stage via `session_replication_role = replica` and unconditionally restored afterwards.
* Non-primary-key unique indexes and unique constraints are dropped before the copy and recreated afterwards. Recreation failures are reported as warnings rather than hard errors; they indicate genuine duplicate values in the source that must be investigated before going live.
* Target PostgreSQL is copied into using per-table transactions.
* On a table failure, that table's transaction is rolled back.
* Application cutover is not part of this tool.
* Errors, warnings, and success messages are colour-coded in the terminal.
* PostgreSQL is prepared to the current runtime schema, while SQL Server is allowed to be an older legacy-shaped source when the tool has a known compatibility mapping.
* Readiness fails only when the source data cannot be mapped safely into the current PostgreSQL target schema.

## Current exclusions

The tool intentionally skips:

* `Hangfire\*` tables
* `\*ReadModel\*` tables
* `\_\_MigrationHistory`
* `\_\_EFMigrationsHistory`
* `\_\_kitos\_migration\_journal`
* `OrganizationOptions`

Read-model data should be rebuilt after a successful migration instead of copied directly.

## Legacy SQL Server source compatibility

The tool can resolve a limited set of legacy SQL Server shapes into the current PostgreSQL schema:

* Known table renames such as `LocalFrequencyTypes -> LocalRelationFrequencyTypes` and `SsoOrganizationIdentities -> StsOrganizationIdentities`
* Known column renames such as `ArchiveSupplierId <- SupplierId`
* EF6-style foreign key names such as `SensitivePersonalDataType_Id -> SensitivePersonalDataTypeId`
* Bridge-column fallbacks when both old and new SQL Server columns exist, using the new column first and the legacy column when the new column is null
* Targeted data backfills for known legacy gaps, such as copying `Advice.ObjectOwnerId` into `AdviceSents.ObjectOwnerId` and `AdviceSents.LastChangedByUserId` when the source child row is null

If a source table or column cannot be resolved through those rules, readiness still fails fast.

Self-referencing foreign keys, such as `User.ObjectOwnerId` and `User.LastChangedByUserId`, are copied in two phases: the table rows are inserted first, and the self-referencing columns are updated afterwards once all rows exist in PostgreSQL.

## Switching the application runtime between SQL Server and PostgreSQL

The KITOS web application selects a database provider at startup through the `Database:Provider` configuration key. The two provider values are `SqlServer` (default) and `PostgreSql`.

### Option 1 – ASP.NET Core environment overlay (recommended for local development)

`Presentation.Web/appsettings.json` configures SQL Server by default.
`Presentation.Web/appsettings.PostgreSql.json` overlays the PostgreSQL provider and connection string.

The overlay is activated by setting `ASPNETCORE_ENVIRONMENT` to `PostgreSql` before starting the application.

`launchSettings.json` already contains profiles for both:

| Profile | Environment | Provider |
|---|---|---|
| `Presentation.Web` | `Development` | SQL Server |
| `IIS Express` | `PostgreSql` | PostgreSQL |

Select the profile in Visual Studio, or pass it on the command line:

```powershell
# SQL Server (default)
dotnet run --project Presentation.Web/Presentation.Web.csproj --launch-profile "Presentation.Web"

# PostgreSQL
dotnet run --project Presentation.Web/Presentation.Web.csproj --launch-profile "IIS Express"
```

### Option 2 – Environment variables (CI, containers, scripted deployments)

Set the provider and connection string directly via environment variables. These take precedence over `appsettings.json`.

```powershell
# Switch to PostgreSQL
$env:Database__Provider              = "PostgreSql"
$env:ConnectionStrings__KitosContext = "Host=127.0.0.1;Port=5432;Database=kitos;Username=postgres;Password=postgres"

# Switch back to SQL Server
$env:Database__Provider              = "SqlServer"
$env:ConnectionStrings__KitosContext = "Server=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Kitos;MultipleActiveResultSets=True;TrustServerCertificate=True"
```

### Switching the provider in a deployed environment

`DeploymentScripts/PreparePackage.ps1` stamps `appsettings.json` with runtime values before the deployment package is created. Setting the `KitosDbProvider` CI/CD environment variable to `PostgreSql` will make the script write that value into `Database.Provider` and `Hangfire.Provider` in the packaged `appsettings.json`; omitting the variable leaves the fields at their `SqlServer` defaults so existing SQL Server deployments are unaffected.

The recommended transition sequence for each environment is:

1. Run the migration tool against that environment's source SQL Server database to populate the PostgreSQL target.
2. Set `KitosDbProvider = PostgreSql` alongside the PostgreSQL connection strings (`KitosDbConnectionStringForIIsApp`, `HangfireDbConnectionStringForIIsApp`) in the CI/CD pipeline for that environment.
3. Deploy and validate. SQL Server remains untouched and can be used to roll back by reverting the three variables above.
4. Once confidence is established, remove the SQL Server infrastructure for that environment and remove the `KitosDbProvider` variable — setting `SqlServer` as the hard-coded default in `appsettings.json` at that point makes the choice permanent.

`DbMigrations.ps1` also reads `Database__Provider` to determine how to run EF Core migrations during deployment, so set that variable in the same pipeline step as `KitosDbProvider` when switching an environment.

### Running integration tests against a specific provider

Two `.runsettings` files are provided at the repository root:

| File | Provider |
|---|---|
| `local-sqlserver.runsettings` | SQL Server |
| `local-postgres.runsettings` | PostgreSQL |

Activate one in Visual Studio via **Test → Configure Run Settings → Select Solution Wide runsettings File**, or pass it to `dotnet test`:

```powershell
# Run integration tests against PostgreSQL
dotnet test Tests.Integration.Presentation.Web --settings local-postgres.runsettings

# Run integration tests against SQL Server
dotnet test Tests.Integration.Presentation.Web --settings local-sqlserver.runsettings
```

### Running EF Core migrations against a specific provider

`dotnet ef` reads the provider and connection string from environment variables through `KitosContextDesignTimeFactory`:

```powershell
# PostgreSQL
$env:Database__Provider              = "PostgreSql"
$env:ConnectionStrings__KitosContext = "Host=127.0.0.1;Port=5432;Database=kitos;Username=postgres;Password=postgres"
dotnet ef database update --project Infrastructure.DataAccess --startup-project Presentation.Web

# SQL Server
$env:Database__Provider              = "SqlServer"
$env:ConnectionStrings__KitosContext = "Server=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Kitos;MultipleActiveResultSets=True;TrustServerCertificate=True"
dotnet ef database update --project Infrastructure.DataAccess --startup-project Presentation.Web
```
