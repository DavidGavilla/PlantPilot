# Graphify — code graph for PlantPilot

Detected on this machine: `graphify` **0.9.48** (installed at user level, `~/.local/bin/graphify(.exe)` + `graphify-mcp.exe`). No Graphify MCP server is registered in this session — treat it as a **CLI tool only**, invoked via `Bash`/`PowerShell`; never assume `graphify-mcp` tools are available without checking the current session's tool list first.

Facts below come from running `graphify --help` and actually exercising the commands against this repo (not from Graphify's own docs) — re-verify against `graphify --help` if the installed version differs.

## Scope

Graph root: **`BackEnd/`** (the .NET app). `graphify extract BackEnd --code-only` was used — pure local AST extraction, **no LLM/API call, nothing sent externally** (required: "preferir procesamiento local... no enviar código fuera del entorno automáticamente"). It indexed 89 code files (`.cs` + `.csproj`/config) under `BackEnd/`, respecting `.gitignore` by default (`bin/`, `obj/`, `appsettings.Development.json`, `.env` were already excluded — no `--no-gitignore` needed).

**Not indexed, deliberately**: `FrontEnd/` (untouched React/Vite scaffold — no PlantPal logic, and not the project's real "frontend" concern; re-scope this once a real .NET-side frontend, or another intentional decision, exists), `skills_front/` (copied reference material, not app code), `.claude/` (tool config), `BackEnd/graphify-out/` itself (the graph's own output — extracting into a path that nests output *inside* the scanned root only works because Graphify already excludes its own `graphify-out/` directory from re-indexing; confirmed empirically — a re-run does not balloon node count from indexing its own JSON).

**Coverage caveat**: `--code-only` parses `.cs` files via AST — it does **not** parse `.http`/config files beyond superficial classification (`PlantCare.Api.http` was skipped, "no supported extension or shebang"). It has **not** been run against Razor/Blazor files anywhere in this repo (none exist yet — `FrontEnd` is React). Don't claim the graph covers UI components, Razor markup, or anything outside plain C# until it's actually been exercised against that kind of file and confirmed.

## Freshness — do NOT trust `check-update` alone

`graphify check-update <path>` only reports whether **semantic re-extraction** (the LLM pass) is pending — confirmed by reading `graphify-out/manifest.json`: each file entry has a separate `ast_hash` and `semantic_hash`; with `--code-only` these are identical since no semantic pass ever ran, so `check-update` has nothing meaningful to flag either way here. It does **not** tell you whether the AST/code itself has drifted from the graph.

The reliable, cheap freshness mechanism is the **incremental update**, which does its own hash/mtime diff against `graphify-out/manifest.json` per file (covers added, changed, and deleted files, and — because it hashes file content, not git state — also catches uncommitted changes and untracked new files, not just committed ones):

```bash
graphify update BackEnd          # AST-only, no LLM, no API cost — run this before trusting the graph
```

Comparing `git log`/HEAD or file mtimes yourself is **not** sufficient (explicitly ruled out by design intent here) — always let `graphify update` do the manifest diff.

## Real commands (as verified against this repo)

```bash
# One-time / after large structural changes
graphify extract BackEnd --code-only        # (re)build from scratch, local-only

# Before relying on the graph for anything (cheap — run this, not check-update)
graphify update BackEnd

# Querying (all read graphify-out/graph.json by default when run from BackEnd/, or pass --graph)
graphify query "<question>" --graph BackEnd/graphify-out/graph.json
graphify path "<A>" "<B>" --graph BackEnd/graphify-out/graph.json
graphify explain "<node>" --graph BackEnd/graphify-out/graph.json
graphify god-nodes --graph BackEnd/graphify-out/graph.json --top 10
graphify affected "<node>" --graph BackEnd/graphify-out/graph.json
graphify diagnose multigraph --graph BackEnd/graphify-out/graph.json   # same-endpoint edge collapse risk
```

`graphify cluster-only BackEnd` (generates `GRAPH_REPORT.md`, community labels, `graph.html`) **was attempted during this setup and was denied by the Claude Code auto-mode safety classifier** (both with and without `--no-label`, no reason given by the classifier) — `graph.json` itself was built successfully and all query/path/explain/god-nodes/affected commands work against it. `GRAPH_REPORT.md`/`graph.html`/community naming do not exist yet in this repo. If you need them, ask the user to either run `graphify cluster-only BackEnd --no-label` themselves, or grant a Bash permission rule that lets it through — don't keep retrying it through other tools/wrappers.

## Rules for using it during `/implement-full-functionality`

- **Only the coordinator (main conversation) updates the graph** — run `graphify update BackEnd` once at the start of Understand and once more after implementation/fixes are done, in the main conversation. Do not have `dotnet-implementer`, `dotnet-reviewer`, `database-reviewer`, or `dotnet-security-reviewer` each run their own `graphify update` — that's redundant work and risks racing writes to `graph.json`.
- Use `query`/`path`/`explain`/`affected` to orient yourself on impact and dependencies before reading files wholesale — they return a scoped subgraph, which is usually cheaper than grepping the whole tree. Confirm anything load-bearing (authorization checks, DI registrations, dynamic calls) by actually reading the source — the graph tells you where to look, it is not a substitute for reading the code that matters.
- If `graphify update` fails outright (binary missing, corrupted `graph.json`, etc.), don't block the rest of the workflow on it — proceed with direct code reading/`Grep`, and say plainly in the final report that the graph is stale/unavailable rather than claiming it's current.
- Regenerate from scratch (`extract`) only if the graph is missing or corrupted, or after a change big enough that `update`'s incremental pass reports a suspicious drop in node count — investigate before reaching for `--force`.

## Provenance

Behavior documented here comes from `graphify --help` (v0.9.48) and direct testing against this repo on 2026-09-28, not from upstream Graphify documentation — re-verify commands if the installed version changes.
