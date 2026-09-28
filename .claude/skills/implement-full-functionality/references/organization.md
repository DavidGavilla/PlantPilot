# Folder, code, and documentation organization

The real, observed convention in `BackEnd/PlantCare.Api` (verified by listing every folder, not assumed) — follow it, don't invent a different one, and don't reorganize the repo to implement a single feature.

## 1. The existing architecture (as it actually is)

**Layer-first, then feature-area subfolder**: `Controllers/<Area>`, `Services/<Area>` + `Services/Interfaces/<Area>`, `Models/<Area>`, `DTOs/<Area>`, `Mappers/<Area>`, `Validators/<Area>`. Each area folder's name is also its C# sub-namespace (`Models/Devices/*` → `namespace PlantCare.Api.Models.Devices;`, and so on) — folder path and namespace must match, they're not independent choices.

**Known, pre-existing exception — the `User` area**: unlike every other area, it's inconsistent *within itself* across layers:
- `DTOs/UserDTO.cs`, `DTOs/RegisterUserDTO.cs`, `DTOs/LoginUserDTO.cs`, `DTOs/UpdateUserDTO.cs` — flat in `DTOs/`, but their namespace is already `PlantCare.Api.DTOs.Users` (plural, as if a subfolder existed).
- `Mappers/UserMapper.cs` — flat, namespace `PlantCare.Api.Mappers` (no area sub-namespace at all).
- `Models/Users.cs` — flat, namespace `PlantCare.Api.Models` (no area sub-namespace at all).
- `Services/Interfaces/User/IUserService.cs` — **does** have a subfolder, but singular (`User`), while the DTO namespace already committed to plural (`Users`).

**Do not move or rename the four existing flat files, or `Services/Interfaces/User/`, to "fix" this** — that's an out-of-scope repo-wide reorg (see §6). For **new** User-area files, follow the dominant convention and the DTO layer's own already-chosen name: `DTOs/Users/`, `Mappers/Users/`, `Models/Users/`, `Services/Users/` + `Services/Interfaces/Users/` (plural, matching `PlantCare.Api.DTOs.Users` and the plural convention every other area already uses — `Devices`, `Plants`; `Diagnosis`/`Schedule` are the only singular area-folder names, both pre-existing, don't generalize from those two). If a feature touches the *existing* flat User files directly (e.g. adding a field to `UserDto`), edit them in place — don't relocate them as a side effect of an unrelated change.

**No generic dumping-ground folders exist today** (`Helpers/`, `Utils/`, `Common/`, `Shared/`) — keep it that way. Don't create one. If code is genuinely needed by more than one area, decide where it actually belongs (often the more foundational area, e.g. a cross-entity validation rule might belong next to the entity it primarily constrains) rather than inventing a new bucket — and only extract it once there are at least two real call sites, not preemptively.

**Frontend**: `FrontEnd/` is a separate React/Vite project with its own conventions (untouched scaffold today) — this policy governs the .NET backend; don't apply C# area-folder conventions to it, and don't restructure it as part of unrelated backend work.

## 2. Placing new code

1. Identify the area (`Devices`, `Plants`, `Diagnosis`, `Schedule`, `Users`, or a genuinely new one) and the layer (`Controllers`/`Services`/`Models`/`DTOs`/`Mappers`/`Validators`) — almost every file belongs at the intersection of an existing area and an existing layer folder.
2. A **new area** (a feature that doesn't map to an existing one) gets its own subfolder in each layer it touches, named consistently and matching a namespace of the same name — don't force it into an unrelated existing area folder.
3. Match the naming already used one level up: `XxxController`, `XxxService`/`IXxxService`, `XxxDto`/`CreateXxxDto`/`UpdateXxxDto`, `XxxMapper` (static extension methods), `XxxDtoValidator`.
4. **No new project, layer, or interface without a demonstrated need.** This repo deliberately has two projects (`PlantCare.Api`, `PlantCare.Api.Tests`) and no repository/unit-of-work abstraction — a feature request is not, by itself, a reason to add either.

## 3. Tests and migrations

- Tests live in `PlantCare.Api.Tests`, mirroring the production project's folder structure so a test's location tells you what it covers (e.g. `Validators/Diagnoses/` mirrors `PlantCare.Api/Validators/Diagnoses/`). A new test area gets the same subfolder name as its production counterpart.
- Migrations and the model snapshot stay exactly where EF Core puts them for this project's single `AppDbContext` — `BackEnd/PlantCare.Api/Migrations/`. Never create a second migrations folder, and never hand-move a migration file.
- **Never mix test output, coverage results, or temporary scripts into production or test source folders.** `TestResults/`, `bin/`, `obj/` are already gitignored; a throwaway script belongs in a scratch location outside the repo, not committed alongside real code.

## 4. Documentation

No `docs/` folder and no root `README.md` exist yet (verified — `FrontEnd/README.md` is the only one, and it's the default Vite template, unrelated to the backend). Don't create either speculatively; create a `docs/<category>/` file only when a specific piece of content actually needs a durable home outside `CLAUDE.md`:

- `docs/architecture/` — a real architectural decision worth recording (e.g. "why services talk to `AppDbContext` directly instead of a repository layer" — currently only stated inline in `CLAUDE.md`; move it here only if it grows into something CLAUDE.md's brief format can't hold).
- `docs/features/` — behavior/contracts for a feature complex enough to need more than the code + its tests to explain (most CRUD endpoints don't).
- `docs/operations/` — configuration, running, migrating, recovering. Right now this lives in `CLAUDE.md`'s "Real commands" section and the `references/` files here; only split it out once it outgrows a quick-reference.
- `docs/development/` — dev setup/checks beyond what `CLAUDE.md` already states.

**Before creating a new doc, check whether an existing one (`CLAUDE.md`, a `docs/*` file, or a `references/*.md` file in this skill) already covers the topic — update it instead of adding a parallel one.** Never create an empty doc "for later," and never create one doc per small change — a one-line behavior tweak doesn't need its own `docs/features/` file, an entry in the PR/commit description is enough.

**Division of responsibility** (don't duplicate the same fact in more than one place):
- `README.md` (when it exists): a brief entry point for a human — what the project is, how to run it, link onward. Not agent instructions, not the full architecture.
- `CLAUDE.md`: instructions for the agent — observed architecture, real commands, when to use this skill. Kept intentionally brief; links to `docs/*` and `references/*` rather than restating their content.
- `.claude/skills/implement-full-functionality/references/*.md`: the detailed how-to for this skill's own phases (Graphify, security, testing, CI, resilience, migrations, and this file). Not meant to be read by a human browsing the repo — CLAUDE.md points here, not the reverse.
- `docs/*`: durable project documentation meant for humans, once it exists. If something belongs in both a `docs/*` file and `CLAUDE.md`, put the full content in `docs/*` and leave a one-line pointer in `CLAUDE.md`, not the other way around.

## 5. Generated files

- **Graphify**: `BackEnd/graphify-out/` — `graph.json`/`manifest.json`/`.graphify_analysis.json` are committed (they're the point — a shared, reusable graph, and Graphify ships a git merge driver specifically for `graph.json`); `BackEnd/graphify-out/cache/` (per-file AST cache) is gitignored — regenerable, not meant to be shared. See `references/graphify.md`.
- **Test results**: `TestResults/*.trx` — already gitignored; CI uploads them as a workflow artifact instead (`.github/workflows/ci.yml`), not committed.
- **Screenshots / browser-verification output**: none produced yet by this project; if a future feature's verification produces one, it's a throwaway artifact for the delivery report, not a committed file, unless the user explicitly asks to keep it (e.g. under `docs/features/` alongside the feature it documents).
- **Secrets/local config**: `appsettings.Development.json`, `BackEnd/.env`, `.claude/scheduled_tasks.lock`, `.claude/settings.json.graphify-bak` — all already gitignored (see `.gitignore`).
- **Apply `.gitignore` selectively** — don't add a broad pattern that would also catch shared documentation or config that must be versioned (`docs/`, `CLAUDE.md`, `.claude/agents/`, `.claude/skills/`, `.claude/settings.json`, `.github/workflows/` are all meant to be committed — never gitignore any of them to "clean up").

## 6. Final check before delivering a feature

Before reporting a feature done, review every file you created or touched:

- **Location and naming** match §1–§3 above — right area, right layer, right namespace-matches-folder.
- **No duplicates** — didn't recreate a DTO/service/validator that already exists under a slightly different name.
- **No orphans** — no file left behind that nothing references (an unused DTO, a validator never registered, an interface with no implementation left mid-work).
- **No temp files of your own** — no scratch scripts, no stray `.sql`/`.json` dumps committed alongside the feature.
- **Documentation links are real** — if you added or edited a `docs/*`/`CLAUDE.md`/`references/*` cross-reference, the target file actually exists at that path.
- **Docs updated when behavior changed** — if the feature changes something a doc claims, update that doc in the same pass, don't leave it stale.
- **No reorganization outside this feature's scope** — you did not rename/move files unrelated to the task, even if you noticed something untidy along the way (like the `User` area exception above). If you find preexisting disorder outside scope, **name it in the delivery report as a separate, optional improvement — don't act on it uninvited.**
