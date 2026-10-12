# OS2KITOS – Copilot Instructions

OS2KITOS is a Danish municipal IT portfolio management platform. It helps public organizations manage IT systems, contracts, interfaces, and data processing registrations (GDPR). Maintained by STRONGMINDS ApS for OS2.

The legacy UI (AngularJS) lives in this repo. The **new frontend** is maintained separately: https://github.com/os2kitos/kitos_frontend

---

## Scope (in priority order)

1. **Correctness & reliability:** Logic errors, race conditions, data loss, broken edge cases.
2. **Security & privacy:** Injection, auth/z, secret handling, unsafe deserialization, insecure defaults.
3. **Compatibility:** Breaking changes, backwards compatibility, input/output validation.
4. **Performance:** Changes that meaningfully impact latency, memory, or scale.
5. **Testing:** Gaps for critical paths; propose **minimal** test additions.
6. **Economic token spend:** Be concise. No explanations unless asked. Code only for generation tasks. Bullets over paragraphs.

---

## Build & Test

Build the solution with MSBuild or Visual Studio:
```
msbuild KITOS.sln
```

**Unit tests** (xUnit, no external dependencies):
```
dotnet test Tests.Unit.Core.ApplicationServices
dotnet test Tests.Unit.Presentation.Web
```

Run a single unit test class:
```
dotnet test Tests.Unit.Core.ApplicationServices --filter "FullyQualifiedName~ItSystemWriteServiceTest"
```

**Integration tests** require a running KITOS instance and PostgreSQL (default local database: `kitos` on `localhost:5432`). Set `KitosTestEnvironment=Local` (default if unset). Integration tests run sequentially within `[Collection(nameof(SequentialTestGroup))]`.

**Quality gate** (same checks as the `PR quality gate` CI workflow – see `docs/CODE_QUALITY.md`):
```
pwsh ./scripts/quality-check.ps1                          # format + build KITOS.sln & Kitos_PubSub.sln (0 warnings) + conventions + unit tests
pwsh ./scripts/quality-check.ps1 -Fix -SkipBuild -SkipTests  # auto-fix formatting of changed files
```

**EF Core migrations** are in `Infrastructure.DataAccess/Migrations/EfCore/`.
Set `ConnectionStrings__KitosContext` to the target PostgreSQL connection string before using the EF Core CLI:
```
dotnet ef migrations add <MigrationName> --project Infrastructure.DataAccess --output-dir Migrations\EfCore
dotnet ef database update --project Infrastructure.DataAccess
```

Migration scaffolding does not require a database connection. Applying migrations changes the target database; do so only when explicitly requested.

---

## Definition of done (mandatory for AI agents)

Before reporting a code change as complete:

1. Run `pwsh ./scripts/quality-check.ps1` (use `-SkipTests` only while iterating; the final run must include tests).
2. On `FAIL`: fix the root cause and re-run until the gate passes. Formatting: re-run with `-Fix`.
   - **Never** silence the gate: do not add `#pragma warning disable`, `[SuppressMessage]`, `NoWarn`, or relax severities / add baseline entries in `.editorconfig` to get green. If a suppression is genuinely required, add a justification and call it out to the user.
3. Address each `WARN`: add/adjust unit tests for changed logic; for SQL script changes, validate PostgreSQL syntax and behavior. Do not add SQL Server variants.
4. Self-review the diff against the `code-quality` skill (`.github/skills/code-quality/SKILL.md`) for KITOS conventions that analyzers cannot check (`Result`/`Maybe`, authorization, transactions, domain events, mappers).
5. Leave files you touched at least as clean as you found them (no new warnings, no dead code, no debug output).

---

## Architecture

### Layer structure (inner → outer)

| Project | Purpose |
|---|---|
| `Core.Abstractions` | Shared functional types: `Result<T,E>`, `Maybe<T>`, `OperationError`, `OperationFailure` |
| `Core.DomainModel` | Entity classes, domain events, interfaces |
| `Core.DomainServices` | Repository interfaces, domain service contracts |
| `Core.ApplicationServices` | Business logic: read/write services, authorization, mapping models |
| `Core.BackgroundJobs` | Hangfire scheduled jobs |
| `Infrastructure.DataAccess` | EF Core `KitosContext`, `GenericRepository<T>`, PostgreSQL migrations |
| `Infrastructure.Ninject` | DI bindings (Ninject) |
| `Infrastructure.STS.*` | SAML/KOMBIT STS integration (Danish government SSO) |
| `Infrastructure.OpenXML` | Excel export |
| `Presentation.Web` | ASP.NET Web API + MVC host; V1 and V2 API controllers, legacy AngularJS UI |
| `Tests.Unit.Core.ApplicationServices` | Unit tests for application services |
| `Tests.Integration.Presentation.Web` | HTTP-level integration tests |
| `Tests.Toolkit` | Shared test base classes |

### API versioning

Controllers live under `Presentation.Web/Controllers/API/`:
- **V1** – legacy API, used by the old AngularJS UI
- **V2/External/** – public API, decorated with `[PublicApi]`, inherits `ExternalBaseController`
- **V2/Internal/** – internal API for the new frontend, inherits `InternalApiV2Controller`

Each V2 domain area has paired mapper types:
- `IXxxResponseMapper` / `XxxResponseMapper` – domain entity → API response DTO
- `IXxxWriteModelMapper` / `XxxWriteModelMapper` – API request DTO → domain write model

### Database

The solution has migrated to **PostgreSQL with EF Core and Npgsql**. PostgreSQL is the only supported database for new work; always use EF Core, never EF6. Do not add SQL Server/MSSQL compatibility paths, SQL dialect variants, or provider tests.

`KitosContext` is the single EF Core `DbContext`. Access goes through `IGenericRepository<T>` → `GenericRepository<T>`. Entity mappings use `IEntityTypeConfiguration<T>` in `Infrastructure.DataAccess/Mapping/`. All schema changes use EF Core migrations in `Infrastructure.DataAccess/Migrations/EfCore/`, including the generated `.Designer.cs` and `KitosContextModelSnapshot.cs`.

EF6 migrations directly under `Infrastructure.DataAccess/Migrations/` and remaining SQL Server tooling are historical migration artifacts, not the current persistence model. Do not modify or use them for new schema changes. Verify mappings against the PostgreSQL schema and current EF Core snapshot; do not infer current column names from historical EF6 conventions. PostgreSQL quoted identifiers are case-sensitive.

Production and staging retain their databases; local and development databases are recreated on every deployment. When correcting a mapping to a column already present in production and staging, do not add a migration solely to rename that column. Align the mapping, current snapshot and database-creation baseline instead, and test the recreated schema. Actual changes to the production/staging schema still require EF Core migrations.

### Option types

There are **Global** option types (system-wide, managed by global admins) and **Local** option types (per-organization customizations). Both follow a `OptionEntity` → `LocalOptionEntity` inheritance pattern. Controllers for these follow the `BaseGlobalRegularOptionTypesInternalV2Controller` / `BaseLocalRegularOptionTypesInternalV2Controller` base class pattern.

---

## Key Conventions

### Error handling: `Result` and `Maybe` monads

Never throw exceptions for business logic failures. Application services return `Result<TSuccess, OperationError>`:
```csharp
Result<ItSystem, OperationError> result = _service.GetSystem(uuid);
return result.Match(
    onSuccess: system => Ok(responseMapper.Map(system)),
    onFailure: error => FromOperationError(error)
);
```

`OperationFailure` enum maps directly to HTTP status codes: `NotFound` → 404, `Forbidden` → 403, `Conflict` → 409, `BadInput` → 400.

Use `Maybe<T>` for optional values (instead of `null`). Implicit conversion from `T` works: `T value = null` becomes `Maybe<T>.None`.

### Unit test pattern

Unit test classes extend `WithAutoFixture` from `Tests.Toolkit`. Use `A<T>()` to create test data and `Many<T>()` for collections. Mock with Moq; inject via constructor in `public TestClassName()`.

```csharp
public class MyServiceTest : WithAutoFixture
{
    private readonly MyService _sut;
    private readonly Mock<IDependency> _depMock;

    public MyServiceTest()
    {
        _depMock = new Mock<IDependency>();
        _sut = new MyService(_depMock.Object);
    }
}
```

Test methods follow the `MethodName_Condition_ExpectedResult` naming pattern (e.g., `Can_GET_Public_ItSystem_As_Stakeholder_If_Placed_In_Other_Organization`).

### Transactions

Use `ITransactionManager` for explicit transaction boundaries in write operations. Services receive it via constructor injection and call `_transactionManager.Begin()` / `_transactionManager.Commit()`. Do not call `SaveChanges()` directly — let the transaction manager handle it.

### Domain events

Domain changes publish events via `IDomainEvents`. Common events: `EntityCreatedEvent<T>`, `EntityUpdatedEvent<T>`, `EntityBeingDeletedEvent<T>`. Raise events from application services, not domain entities directly.

### Authorization

Authorization goes through `IAuthorizationContext` (injected into services). Check permissions before mutating entities. `IOrganizationalUserContext` provides the current user's organizational role and memberships.

### Read models

For complex queries (e.g., grid/list views), denormalized read models (e.g., `ItSystemUsageOverviewReadModel`, `ItContractOverviewReadModel`) live in `Core.DomainModel` and are kept up-to-date via background jobs. Prefer read models over ad-hoc joins for list endpoints.

### Naming

- Controllers: `{Domain}V2Controller` (external) or `{Domain}InternalV2Controller` (internal)
- Services: `I{Domain}Service` / `I{Domain}WriteService`
- Mappers: `I{Domain}ResponseMapper` / `I{Domain}WriteModelMapper`
- Tests: `{ClassUnderTest}Test`

### Danish domain terms

- **KLE** – *Kommunernes Landsforenings Emnesystem*: task/topic classification system used by Danish municipalities
- **DPR** – Data Processing Registration (GDPR data processor agreements)
- **STS** – *Security Token Service*: KOMBIT's Danish government SSO/federated identity
- **ItSystemUsage** – an organization's local registration/usage of a globally defined `ItSystem`
