# CI/CD — GitHub Actions, .NET 8, SQL Server

Adapted from `codewithmukesh/dotnet-claude-kit`, skill `ci-cd` (MIT License, © 2025 Mukesh Murugan, https://github.com/codewithmukesh/dotnet-claude-kit). Reused: core principles (pipeline as code, fast feedback, build-once-deploy-many, tests always gate), the GitHub Actions structure, and the anti-patterns list. **Changed for this repo**: the source targets **.NET 10** and a **PostgreSQL** service container — this repo is **.NET 8**, **SQL Server** (`mcr.microsoft.com/mssql/server:2022-latest`, matching `BackEnd/docker-compose.yml`), and has **no separate Infrastructure project** (EF Core lives directly in `PlantCare.Api`), so the workflow below uses `working-directory: BackEnd` and per-project commands instead of `--project`/`--startup-project` flags. Azure DevOps is not covered — the repo's remote is `github.com/DavidGavilla/PlantPilot`, so GitHub Actions is the real provider (verified via `git remote -v`), not assumed.

## What exists now

`.github/workflows/ci.yml` — created as part of this setup (there was no prior CI to reuse; confirmed via `find . -path "*/.github/workflows/*"` returning nothing before this change). Triggers on push/PR to `main`. Steps: checkout → setup .NET 8 → restore/build (`BackEnd/`) → start & wait for a SQL Server service container → `dotnet tool restore` + `dotnet ef database update` (applies real migrations, the same way `dotnet-implementer`/`database-reviewer` are expected to verify them locally) → `dotnet test` with TRX results uploaded as an artifact → `dotnet list package --vulnerable --include-transitive` per project (no `.sln` exists, so it must run per-project, not once from `BackEnd/` — confirmed by testing locally, `dotnet list package` from a directory with two `.csproj` and no `.sln` fails).

## What's verified vs. not

- **Config created**: yes, `.github/workflows/ci.yml`.
- **Commands tested locally**: yes — `dotnet build`, `dotnet ef database update` (against the local Docker SQL Server), `dotnet test`, and `dotnet list package --vulnerable --include-transitive` (per-project) were all run directly in this environment during setup, not just written and assumed to work.
- **Pipeline actually executed on GitHub Actions**: **no** — nothing has been pushed, so the workflow has never actually run on a GitHub-hosted runner. Don't claim "CI passes" until it has actually run there.

## Known limitation: the vulnerability check doesn't gate the build

`dotnet list package --vulnerable` exits `0` even when it finds vulnerable packages (confirmed empirically) — it's informational only in the current workflow, not a hard gate. It already found a real, pre-existing issue: `PlantCare.Api.Tests` pulls in `System.Net.Http` 4.3.0 and `System.Text.RegularExpressions` 4.3.0 as **HIGH**-severity transitive vulnerable packages (via `xunit`/`coverlet.collector`'s dependency chain) — reported to the user, not fixed as part of this setup (out of scope: fixing it means bumping/pinning a package and re-testing, which is an implementation change, not skill/config setup). If a hard gate is wanted later, it needs to parse the command's text output (no clean machine-readable flag exists) or switch to a dedicated tool (e.g. `dotnet-outdated`, GitHub's own Dependabot) — don't silently add a brittle text-parsing gate without flagging that tradeoff.

## Anti-patterns (kept from source, still apply)

- **Don't build different artifacts per environment** — one `Release` build, promoted, not a different config per env.
- **Don't skip format checks silently** — `dotnet format --verify-no-changes` was tested locally during this setup and **currently fails** with pre-existing whitespace violations across ~8 files not touched by this task. It was deliberately **left out of `ci.yml`** rather than added and immediately red, and rather than added with `continue-on-error` (explicitly disallowed). Pending: run `dotnet format` to fix the existing violations (a separate, scoped change), then add the check as a real gate.
- **Don't hardcode real secrets in the pipeline** — the SQL Server SA password in `ci.yml` is an intentional exception: it's an ephemeral, job-scoped container password with no existence outside that one CI run, not a credential to anything real. Don't reuse it, and don't add an actual secret (API keys, deployment credentials) to the YAML directly — use repository secrets (`${{ secrets.NAME }}`) for anything that isn't disposable.

## Per-feature checklist (what the coordinator does in Section 8 of a feature)

1. Does this change need a new CI step (new project, new external service dependency, new package to scan)? Most feature-level changes don't — they just need the existing `build` → `migrate` → `test` steps to keep passing.
2. Reuse `.github/workflows/ci.yml` — don't create a second, parallel workflow file for routine feature work.
3. Never weaken an existing check (`continue-on-error: true`, deleting/skipping a test, narrowing what's scanned) to make the pipeline green — fix the actual failure or, if it's genuinely out of scope, say so explicitly instead of hiding it.
4. State plainly, every time: config created vs. commands verified locally vs. pipeline actually run on GitHub — never claim the third without having triggered it.
