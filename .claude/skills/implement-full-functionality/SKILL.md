---
name: implement-full-functionality
description: Coordinates end-to-end implementation of a PlantPilot .NET backend (and .NET-side frontend, once one exists) feature — graph-informed understanding, requirements traceability, upfront security/failure-mode assessment, implementation via dotnet-implementer, tests, independent review (dotnet-reviewer, database-reviewer, dotnet-security-reviewer), CI awareness, and a final graph update — before reporting what's implemented, tested, reviewed, and pending. Invoke explicitly with /implement-full-functionality <feature description>.
disable-model-invocation: true
---

You coordinate a full feature end to end for PlantPilot, invoked as `/implement-full-functionality <description>`. **Run this coordination yourself, in this conversation** — dispatch and collect subagent results here; never hand the whole task to one subagent and return its result unread, and never run this as an isolated background job.

Stack (verified, not assumed — see `CLAUDE.md` for the full picture): .NET 8, ASP.NET Core (`BackEnd/PlantCare.Api`), EF Core 8.0.16 / SQL Server (`BackEnd/docker-compose.yml`), xUnit 2.5.3 + FluentValidation. **Frontend**: `FrontEnd/` is React + Vite — **not** Blazor/Razor Pages/MVC. Don't invent a .NET frontend that doesn't exist; if a feature has a UI component, say plainly that this repo currently has no .NET-side frontend to apply it to, rather than forcing React work or fabricating Razor files. All application code stays in the existing .NET stack — the tools below (Graphify, GitHub Actions runners, Testcontainers) may use their own runtimes without that counting as "introducing a new app framework."

References (read on demand, per phase below — don't load all of them into every run):
`references/graphify.md`, `references/requirements-traceability.md`, `references/security.md`, `references/testing.md`, `references/ci-cd.md`, `references/resilience.md`, `references/migrations.md`, `references/organization.md`.

Agents: `dotnet-implementer` (read/write, implements), `dotnet-reviewer` / `database-reviewer` / `dotnet-security-reviewer` (all read-only). `design-director` is **not installed in this project** (checked all configured marketplaces) — if a feature has a visible UI component, say so plainly instead of claiming a design review happened.

## 1. Inspection + graph

Read `CLAUDE.md` and the code actually relevant to the request (`Controllers/`, `Services/`, `Models/`, `DTOs/`, `Mappers/`, `Validators/` for the affected area — several areas already have DTOs/mappers/validators but no service/controller, check `Services/Interfaces/*` first). Then, per `references/graphify.md`: run `graphify update BackEnd` (cheap, local, no LLM) — **not** `check-update` alone, it only flags pending semantic re-extraction, not code drift. If the graph is missing/corrupted, `graphify extract BackEnd --code-only` rebuilds it; if `graphify` itself fails, say so and proceed with direct reading instead of blocking. Use `graphify query`/`path`/`explain`/`affected` to scope impact before reading files wholesale — confirm anything load-bearing (auth/isolation, DI, dynamic calls) by reading the actual source. **Only you (the coordinator) update the graph** — subagents don't run their own `graphify update`.

Run the baseline (`cd BackEnd && dotnet build PlantCare.Api && dotnet test PlantCare.Api.Tests`) so you can tell a pre-existing failure from one you introduce.

## 2. Requirements → acceptance criteria

Build the table from `references/requirements-traceability.md`: stable IDs, expected behavior, affected components, planned test/check, evidence, status. Only explicit requirements + their necessary technical corollaries get an ID — don't invent extra scope. Include negative cases (wrong/missing `userId`, invalid input, another user's resource) as their own rows.

## 3. Security and failure-mode assessment (upfront)

`references/security.md` Part 1 — proportional to the change. Note untrusted input, sensitive data, whose data this is and how it's isolated (this app has no auth; isolation = explicit `userId` filtering), side effects, external dependencies, and the one or two real risks + what in the design prevents them. If the feature has side effects worth retry/idempotency thought (external API call, device command), read `references/resilience.md` now, not after implementation.

## 4. Contracts and plan

Concrete steps per layer, migration or not, test plan. Decide file/folder placement now, per `references/organization.md` (area-then-layer convention, no generic Helpers/Utils/Common, no new project/layer without a demonstrated need) — don't leave it for `dotnet-implementer` to improvise per task. Resolve reversible decisions yourself; only ask the user for genuine ambiguity or a real conflict with an existing constraint. No microservices/CQRS/generic repositories/new layers without a demonstrated need — this codebase keeps services talking to `AppDbContext` directly.

## 5. Implementation (backend / frontend)

Dispatch well-bounded tasks to `dotnet-implementer`, telling it exactly which files/folders to create per `references/organization.md`. **Never two tasks touching the same file, never two migration-generating tasks concurrently** — serialize anything touching `AppDbContext.cs`/`Models/`. Wait for and integrate each result before dispatching the next dependent one. Model/migration changes follow `references/migrations.md`. If the feature has a UI part, see the frontend note above — there is no `design-director` and no .NET frontend yet; don't fabricate either.

## 6. Tests and integration

Per `references/testing.md`. Tests proportional to risk from the requirements table — unit for isolated rules, integration (`WebApplicationFactory` + `Testcontainers.MsSql`, never `UseInMemoryDatabase` for anything relational) for endpoint behavior and the negative/isolation cases. No packages installed speculatively — add the one a specific test needs, when it needs it.

## 7. Review — functional, data, security (and visual if applicable)

Get the diff (`git diff`/changed-file list) and dispatch **in parallel**: `dotnet-reviewer` (functional defects, authorization/isolation, async correctness, data exposure, regressions) always; `database-reviewer` (entities, cardinalities, migrations) if `Models/`/`AppDbContext.cs`/a migration changed; `dotnet-security-reviewer` (`references/security.md` Part 2 — OWASP patterns, secrets, dependency CVEs, CORS, formal severity) always as the final independent pass. They're all read-only and independent — don't have them repeat each other's full sweep (see `dotnet-security-reviewer.md`'s own scope-discipline note). Visual review only if the feature has a UI and a UI reviewer is actually available (it currently isn't).

## 8. Fixes and re-checks

Triage every finding against the real code yourself. Fix what's valid, re-run the specific affected build/test/review — not the entire pipeline from scratch unless the fix was broad. Do not end with a known, in-scope, confirmed Critical/High finding unaddressed; if you're not fixing something, say exactly why.

## 9. Requirements verification + CI

Fill in the evidence/status column for every row in the requirements table — real test names and outcomes, not assumptions. Per `references/ci-cd.md`: does this change need a CI update? Usually no (the existing `build → migrate → test` steps just need to keep passing). Never weaken an existing check to go green. State plainly whether CI config changed, whether commands were verified locally, and whether the pipeline actually ran on GitHub — these are three different claims, don't conflate them.

## 10. Final Graphify update

`graphify update BackEnd` again, once, after all fixes land — not per-subagent. If it fails, say the graph is stale rather than claiming it's current.

## 11. Delivery

Before reporting, run the final-check pass from `references/organization.md` §6 on every file you created/touched (location/naming, no duplicates/orphans/temp files, doc links valid, docs updated if behavior changed, no out-of-scope reorganization) — note any preexisting disorder you noticed but didn't touch as a separate, optional suggestion, never act on it uninvited.

Report: **Implemented** (files per layer), **Requirements table** (final state), **Tested** (real commands + output), **Migrations** (generated/applied, against which DB), **Reviewed** (all four review angles — functional, database if applicable, security, visual if applicable — findings, fixes, anything left open and why), **CI** (created/verified-locally/actually-run, per above), **Graph** (updated or not, why), **Organization** (final-check result, any out-of-scope disorder flagged), **Pending/limitations**.

Never `git commit`, push, deploy, or touch production/shared infrastructure unless explicitly asked in this invocation. If an external tool/permission blocks a step (e.g. a command the environment denies), finish everything else and say exactly what's blocked and why — don't loop retrying the same blocked action.

## Example

```
/implement-full-functionality Add full CRUD for Plants (PlantsController + PlantsService), scoped under api/users/{userId}/plants, using the existing CreatePlantDTO/UpdatePlantDTO/PlantsDTO and validators already in the repo.
```
