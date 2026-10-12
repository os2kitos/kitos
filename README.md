# OS2KITOS

## Maintainer and license info
OS2KITOS is maintained by STRONGMINDS ApS (https://www.strongminds.dk)
for OS2 - Offentligt digitaliseringsfællesskab (https://os2.eu/).

Copyright (c) 2014, OS2 - Offentligt digitaliseringsfællesskab.

The OS2KITOS is free software; you may use, study, modify and
distribute it under the terms of version 2.0 of the Mozilla Public
License. See the LICENSE file for details. If a copy of the MPL was not
distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

All source code in this and the underlying directories is subject to
the terms of the Mozilla Public License, v. 2.0. 

## Solution structure

### Backend
This repository maintains the backend services.

Persistence uses **PostgreSQL with EF Core/Npgsql**. Always use EF Core, never EF6.
PostgreSQL is the only database target for new work; do not add SQL Server compatibility.
EF6 migrations and SQL Server tooling retained in the repository are historical artifacts.
Current entity mappings live in `Infrastructure.DataAccess/Mapping/`; current migrations
and the model snapshot live in `Infrastructure.DataAccess/Migrations/EfCore/`.

### UI
The UI is developed and maintained here: https://github.com/os2kitos/kitos_frontend

## Build and test

Build the solution:

```powershell
msbuild KITOS.sln
```

Run unit tests:

```powershell
dotnet test Tests.Unit.Core.ApplicationServices
dotnet test Tests.Unit.Presentation.Web
```

Run integration tests:

```powershell
dotnet test Tests.Integration.Presentation.Web
```

Run the code quality gate (formatting, 0-warning build of `KITOS.sln` and `Kitos_PubSub.sln`, conventions, unit tests – same as the PR check, see [docs/CODE_QUALITY.md](docs/CODE_QUALITY.md)):

```powershell
pwsh ./scripts/quality-check.ps1
```

### STS organization identity schema

`dbo.StsOrganizationIdentities` uses `OrganizationId` for its organization foreign key.
Production and staging already use this name; no migration is needed for the mapping fix.
Local and development databases are recreated on deployment. The EF Core baseline
creates `OrganizationId` so recreated databases match the application mapping.

### Integration test database

Integration tests require a running KITOS instance and a PostgreSQL database configured through `Tests.Integration.Presentation.Web/Properties/launchSettings.json`.

The default launch profile (`Tests.Integration.Presentation.Web`) now targets **PostgreSQL** locally:

- `ConnectionStrings__KitosContext=Host=localhost;Port=5432;Database=kitos;Username=postgres;Password=localNoSecret`
