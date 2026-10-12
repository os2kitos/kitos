# Code quality

KITOS uses one quality gate for developers, AI agents and CI:

The persistence stack is **PostgreSQL with EF Core/Npgsql**, not EF6 or SQL Server.
New database work must target PostgreSQL only. EF6 migrations and SQL Server tooling
remaining in the repository are historical artifacts, not compatibility requirements.
Create migrations under `Infrastructure.DataAccess/Migrations/EfCore/` and verify
entity mappings against the PostgreSQL schema and current EF Core snapshot.

| Where | How |
|---|---|
| Locally / AI agents | `pwsh ./scripts/quality-check.ps1` |
| Pull requests to `master` | `.github/workflows/pr-quality.yml` (runs the same script) |
| IDE | Rules from `.editorconfig` show live in Visual Studio / Rider / VS Code |

## What is checked

1. **Formatting** – `dotnet format whitespace` on changed `*.cs` files only (legacy files are fixed when touched).
2. **Build with 0 warnings** – `KITOS.sln` and `Kitos_PubSub.sln` are built with `-p:KitosQualityGate=true`, which turns every compiler and analyzer warning into an error. The rules live in `.editorconfig`; `Directory.Build.props` enables the .NET analyzers and code style in build for all projects.
3. **KITOS conventions** on the diff against the merge base with `origin/master` (committed, uncommitted, untracked **and deleted** files):
   - no `Console.Write*` / `Debug.Write*` / `Debugger.Break` in production code (FAIL)
   - warning suppressions need a justification: `#pragma warning disable` with a `// reason` on the same line or the line above; `[SuppressMessage(...)]` (also multi-line) with a non-empty `Justification` (FAIL)
   - added/deleted EF Core migrations come with their `.Designer.cs` and an updated `KitosContextModelSnapshot.cs` (FAIL)
   - added/changed/deleted SQL scripts → verify PostgreSQL syntax, schema compatibility and data preservation; do not add SQL Server variants (WARN)
   - business logic (`Core.*`, `Presentation.Web/Controllers`, `PubSub.Core.*`, `PubSub.Application.*`) changed or deleted without test changes (WARN)
   - analyzer rules relaxed (WARN – needs reviewer approval). The effective severities in every changed, added or deleted `.editorconfig`/`.globalconfig` are compared with the merge base: lowered or removed enforced rules, demoted naming/category-wide severities and changed naming definitions are reported; removing baseline entries or promoting rules is not. Changes to warning/analyzer MSBuild properties (`NoWarn`, `TreatWarningsAsErrors`, `AnalysisLevel`, …), added/deleted `Directory.Build.*` files and changes to the gate itself are reported as well.
4. **Unit tests** – `Tests.Unit.Core.ApplicationServices`, `Tests.Unit.Presentation.Web` and `PubSub.Test`.

The gate stops with an error if the merge base cannot be resolved (e.g. failed fetch), instead of silently checking only local changes.

Useful switches: `-Fix` (apply formatting), `-SkipBuild`, `-SkipTests`, `-BaseRef origin/master`.

Normal local and Docker builds are unaffected – analyzer findings show as warnings there and only fail with `KitosQualityGate=true`.

## Rule policy (ratchet)

`.editorconfig` severities:

- `warning` – enforced. Only rules with **zero** violations in the codebase are enforced, so the build is green from day one.
- `suggestion` – shown in the IDE only. These rules still have existing violations.

To tighten the gate:

1. Pick a `suggestion` rule (or a rule from the `latest-recommended` set), fix its violations.
2. Promote it to `warning` in `.editorconfig` in the same PR.

Never demote a rule to make a PR pass. The **baseline** section in `.editorconfig` lists pre-existing violations in specific files; entries are only removed (when fixed), never added.

## AI agents

`.github/copilot-instructions.md` contains a mandatory *Definition of done*: agents must run the gate, fix failures and re-run until it passes, and self-review using `.github/skills/code-quality/SKILL.md`, which covers KITOS conventions analyzers cannot check (`Result`/`Maybe`, authorization, transactions, domain events, mappers, tests).

## Making the gate required

In GitHub → *Settings → Branches → master* branch protection, add **Format, analyzers, conventions and unit tests** as a required status check.
