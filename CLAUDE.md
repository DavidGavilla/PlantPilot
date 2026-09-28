# PlantPilot

Plant-care tracking app: ASP.NET Core backend + React frontend.

## Stack (observed, not assumed)

- **Backend**: `BackEnd/PlantCare.Api` — ASP.NET Core, **.NET 8** (`net8.0`, no `global.json` pinning a specific SDK patch), C# nullable+implicit usings enabled.
- **Persistence**: EF Core **8.0.16**, provider **SQL Server** (`UseSqlServer`). Local dev DB runs in Docker (`BackEnd/docker-compose.yml`, image `mcr.microsoft.com/mssql/server:2022-latest`, container `plantpilot-mssql`, port 1433). Connection string lives in `BackEnd/PlantCare.Api/appsettings.Development.json` (gitignored — see below) and `BackEnd/.env` (gitignored, holds `MSSQL_SA_PASSWORD`).
- **Tests**: `BackEnd/PlantCare.Api.Tests` — xUnit 2.5.3 + `FluentValidation.TestHelper`, plus `Microsoft.AspNetCore.Mvc.Testing`/`Testcontainers.MsSql` for integration tests (`Plants/` folder — `ApiFixture` spins up its own throwaway SQL Server container per test run, separate from the `plantpilot-mssql` dev container; running `dotnet test` requires Docker to be running). Validator tests still exist too.
- **Validation**: FluentValidation, one validator class per DTO under `Validators/<Area>/`.
- **Frontend**: `FrontEnd` — React 19 + Vite + TypeScript, mostly untouched scaffold (default Vite template). **This is not a .NET frontend** (no Blazor/Razor Pages/MVC exist in this repo) — don't assume one when working on UI-adjacent requests; say so instead. Not covered by the `/implement-full-functionality` skill below.
- **No solution file** (`.sln`) ties the two backend projects together; each `.csproj`/command is targeted individually (`dotnet build PlantCare.Api`, `dotnet list PlantCare.Api package ...`, etc. — running a project-less command from `BackEnd/` fails with two `.csproj` present).
- **CI**: `.github/workflows/ci.yml` (GitHub Actions — the repo's real remote is `github.com/DavidGavilla/PlantPilot`). Build → apply migrations against a SQL Server service container → test → `dotnet list package --vulnerable` per project. Never actually triggered yet (nothing pushed) — verified locally only.
- **Known dependency issue**: `PlantCare.Api.Tests` currently pulls in `System.Net.Http` 4.3.0 and `System.Text.RegularExpressions` 4.3.0 as HIGH-severity vulnerable transitive packages (via `xunit`/`coverlet.collector`). Not fixed yet — flagged by `dotnet list package --vulnerable --include-transitive`, which is now part of CI (informational only — it doesn't fail the build; see `references/ci-cd.md`).

## Architecture (as it actually is, not as it "should" be)

- Layering: `Controllers/<Area>` (thin, `[ApiController]`) → `Services/Interfaces/<Area>/IXxxService` + `Services/<Area>/XxxService` (injects `AppDbContext` directly, **no repository/unit-of-work layer** — keep it that way) → `Models/<Area>` (EF entities) → `Data/AppDbContext.cs`.
- DTOs in `DTOs/<Area>`, mappers in `Mappers/<Area>` as **static extension methods** (`ToDto()`, `ToEntity()`, `UpdateEntity()`).
- Routes nest under the owning resource: `api/users/{userId:int}/devices`.
- **`Program.cs` still registers no DI services for pre-existing unimplemented areas** — `IDeviceService`/`DeviceService` remains unregistered (known, not yet fixed; `DevicesController` 500s at request time as a result). Areas implemented since (`Plants`, `Farms`, `Workspaces`) **are** registered — any further new service must be wired the same way with `builder.Services.AddScoped<IXxx, Xxx>()` or its endpoints will 500.
- **No authentication is configured** (no `[Authorize]`, no JWT/cookie scheme — `Program.cs` calls `UseAuthorization()` with nothing to authorize against). "Authorization" today means explicit application-level checks against real rows taken from the route — either `userId` filtering directly (`Devices`, the older pattern), or, for workspace-scoped areas, a `WorkspaceMember` row lookup for `(userId, workspaceId)` plus a role check (`Plants`/`Farms` — see `PlantsService.cs`/`WorkspaceAccessService.cs` for the reference pattern: no membership → 403, `Member` role restricted to own resources, `Owner`/`Admin` unrestricted within the workspace). Every new query/mutation on a user's or workspace's data must do the same explicit filtering, since nothing else prevents cross-user/cross-workspace access.
- Scaffolded-but-unimplemented areas: `Diagnosis`, `Schedule`, `User` already have DTOs/mappers/validators but **no service implementation or controller** — `Services/Interfaces/*` already defines their contracts. `Devices` is wired (controller + service) but not DI-registered (see above). `Plants` is now the reference implementation for a fully wired, workspace-scoped, role-authorized CRUD area (`Controllers/Plants/`, `Services/Plants/`) — copy its pattern, and `Farms`'/`Workspaces`' read-only selector pattern, for new areas rather than `Devices`' older `userId`-only one.
- EF Core gotchas already hit once in this codebase, worth re-checking on every model change: PK property names not matching `Id`/`{Entity}Id` convention, unbound navigation properties creating duplicate shadow FK columns, SQL Server rejecting FKs that create multiple cascade paths to the same table (needs `DeleteBehavior.Restrict` on one path), `DeleteBehavior.SetNull` on a non-nullable FK column, and missing `HasPrecision` on `decimal` columns. Also: a `Restrict` FK's absence can be masked by an *indirect* cascade path elsewhere in the graph — `PlantsService.DeletePlantAsync`'s hard-delete pre-check looked safe by inspection but still needed a `try/catch (DbUpdateException)` fallback to archiving, because nothing enforces that `Schedule.DiagnosisId`'s diagnosis always belongs to `Schedule.PlantId`'s plant; don't trust a pre-check like that without a runtime safety net.
- **Multi-tenant model**: `Workspace`/`WorkspaceMember` (personal or business), with `Plant`/`Device`/`Farm`/`IrrigationZone`/`Alert` scoped by `WorkspaceId`. `Plants` now enforces this at the service layer (see above); other areas still don't. Cross-workspace links are still **not all** DB-enforced (confirmed, documented) — `IrrigationZonePlant` is the one exception. `Plant` also has an `IsArchived`/`ArchivedAt` soft-delete pair: deleting a plant with dependent Alerts/irrigation links/diagnosis-less Schedules archives it (and cancels its pending automations) instead of a hard delete, which those same FKs would otherwise reject. Full schema, decisions, and migration/backfill details: `docs/architecture/workspace-data-model.md`.
- **Dev CORS**: `Program.cs` allows `http://localhost:5173`/`5174` (Vite dev server) in `Development` only, for the (not-yet-built) frontend to call the API — no wildcard origin.

## Real commands

```bash
# Build / test (from BackEnd/)
dotnet build PlantCare.Api
dotnet test PlantCare.Api.Tests

# Local SQL Server (from BackEnd/)
docker compose up -d          # start
docker compose down           # stop

# EF Core migrations (from BackEnd/PlantCare.Api/, local dotnet-ef tool pinned to 8.0.16 via .config/dotnet-tools.json)
dotnet ef migrations add <Name>
dotnet ef database update
```

## Claude Code setup

- Agents: `.claude/agents/dotnet-implementer.md` (read/write, implements); `dotnet-reviewer.md`, `database-reviewer.md`, `dotnet-security-reviewer.md` (all read-only review — functional/isolation, data model, and OWASP-mapped security respectively).
- Plugins (project-scoped, `.claude/settings.json`, marketplace `dotnet-agent-skills` = `dotnet/skills`): `dotnet`, `dotnet-aspnetcore`, `dotnet-data`, `dotnet-test`. **Pending step for a fresh clone**: the marketplace isn't auto-registered on a new machine, so run once:
  ```
  claude plugin marketplace add dotnet/skills
  claude plugin install dotnet@dotnet-agent-skills --scope project
  claude plugin install dotnet-aspnetcore@dotnet-agent-skills --scope project
  claude plugin install dotnet-data@dotnet-agent-skills --scope project
  claude plugin install dotnet-test@dotnet-agent-skills --scope project
  ```
- Skill: `/implement-full-functionality <description>` (formerly `/implement-functionality` — renamed and expanded) — coordinates a full feature: graph-informed understanding (Graphify, see below) → requirements/acceptance-criteria table → upfront security/failure-mode check → implement via `dotnet-implementer` → migrate → test → parallel review (`dotnet-reviewer`/`database-reviewer`/`dotnet-security-reviewer`) → CI awareness → final graph update → report. Reference material (adapted from `dotnet/skills` and `codewithmukesh/dotnet-claude-kit`, MIT-licensed — provenance noted in each file) lives in `.claude/skills/implement-full-functionality/references/`. Use it for any non-trivial PlantPal backend feature or bug fix touching more than one layer; for a single-file, single-layer tweak, just edit directly.
- **Graphify** (code graph, `~/.local/bin/graphify`, v0.9.48): graph lives at `BackEnd/graphify-out/`, scoped to `BackEnd/` only, built with `--code-only` (local AST, no LLM/external calls). Rules for using/updating it are auto-appended below this section (written by `graphify claude install`) and in `.claude/skills/implement-full-functionality/references/graphify.md` — the latter has the important caveat that `graphify check-update` alone is **not** a reliable freshness check (it only flags pending semantic re-extraction); use `graphify update BackEnd` instead. `graphify cluster-only` (GRAPH_REPORT.md/community naming) was blocked by the Claude Code auto-mode safety classifier during setup and has not been generated — `graph.json` itself works fine for `query`/`path`/`explain`/`god-nodes`/`affected`.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
