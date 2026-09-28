# Security — upfront assessment and final review

Adapted from `codewithmukesh/dotnet-claude-kit`, skill `security-scan` (MIT License, © 2025 Mukesh Murugan, https://github.com/codewithmukesh/dotnet-claude-kit). Reused: the 6-layer structure, OWASP Top 10:2025 mapping, severity-rating discipline, and finding/report format. **Changed for this repo**: Layer 4 no longer assumes `find_references`/`get_endpoint_map` MCP tools (none are configured in this session) — use `Grep` for `[Authorize]`/`[AllowAnonymous]`/`[ApiController]` instead. No `security-auditor` agent, `/verify`, `/health-check`, `authentication`, or `configuration` skill from the source kit are installed or referenced here — this project only pulled `security-scan`. PlantPilot-specific facts (no auth configured yet, `userId`-scoped isolation instead of multi-company/multi-tenant, device/watering-command replay risk) replace the source's generic examples.

## Part 1 — Upfront assessment (before designing a feature)

Proportional to the change — a one-file CRUD endpoint needs a paragraph, not a table. Before implementation starts, note:

- **Untrusted input**: what does this feature accept from a client, and is it validated (FluentValidation) before it reaches a service/query?
- **Sensitive data**: does it touch `PasswordHash`, `ApiKeyHash`, plant/device location, or anything a DTO shouldn't leak back out?
- **Users/roles/devices/workspaces**: whose data is this — does the route/body carry the owning `userId`/`workspaceId` (or a chain to it), and does every query filter by it? PlantPilot has **no authentication today** (see `CLAUDE.md`) — "authorization" is entirely this manual filtering; treat any missing filter as a live cross-user/cross-workspace data-exposure bug, not a theoretical one. **As of the workspace/farm/irrigation data model (see `docs/architecture/workspace-data-model.md`), this codebase does have a tenant concept**: `Workspace` (personal or business), with `WorkspaceMember` granting a user access, and `Plant`/`Device`/`Farm`/`IrrigationZone`/`Alert` all scoped by `WorkspaceId`. No service/endpoint enforces this yet (model/persistence only, no controllers built on it). One cross-workspace join is DB-enforced already (`IrrigationZonePlant`, via a composite FK trick — see the architecture doc); every other workspace-scoped join (`IrrigationZone`/`IrrigationConfig`'s `DeviceChannel` FKs, `PhotoCaptureSchedule`, `Alert`'s resource FKs, `Plant.PlotId`) is **not** blocked at the database level and needs an explicit same-workspace check in application code the moment a service is built on it.
- **Side effects**: does it write to the DB, send a device command (`DeviceCommand`), call an external AI/API (`IPlantHealthAiService`, `IGeminiScheduleService`), or trigger anything hard to undo? See `resilience.md` for idempotency/retry rules on these.
- **External dependencies**: new NuGet package, new outbound HTTP call, new device-facing endpoint?
- **Risks and controls**: the one or two things that could actually go wrong, and what in the design (filter, validator, idempotency key, rate limit) prevents it.

Translate anything non-trivial here into an actual design/test decision (e.g. "endpoint returns a `Device`, so the service must filter by `userId`" → becomes an acceptance criterion in `requirements-traceability.md` and a negative test case).

## Part 2 — Final review (dispatched to `dotnet-security-reviewer`)

### Layers

| # | Layer | OWASP 2025 | Method (adapted — no MCP assumed) |
|---|-------|-----------|--------|
| 1 | Package vulnerabilities | A03 Supply Chain | `dotnet list package --vulnerable --include-transitive` (run from `BackEnd/`, both `.csproj`) |
| 2 | Secrets detection | — | `Grep` over `.cs`/`.json`/`.yml`/`.xml`/`.config`/`.env*` for the patterns below |
| 3 | OWASP code patterns | A05 Injection, A08 Integrity, A04 Crypto, A01 Access Control | `Grep`/`Read` the diff for raw SQL, `Html.Raw`, `BinaryFormatter`, MD5/SHA1, missing ownership filters |
| 4 | Auth configuration | A07 Authentication, A01 Access Control | `Grep` for `\[Authorize\]`/`\[AllowAnonymous\]`/`\[ApiController\]` across `Controllers/`; today this project has **no auth scheme at all** (`Program.cs` calls `UseAuthorization()` with nothing to authorize against) — Layer 4 today is really "does every action filter by the owning `userId`?", not JWT/policy review. Re-activate the JWT checklist below the day authentication is actually added. |
| 5 | CORS policy | A02 Misconfiguration | `Grep` `Program.cs` for `AddCors`/`UseCors` — none configured yet; flag if a wildcard-origin policy is introduced |
| 6 | Data protection | A04 Crypto, A09 Logging & Alerting | Entities/DTOs returned by controllers (no `PasswordHash`/`ApiKeyHash` leaking), `Grep` for `LogInformation`/`LogWarning` calls that interpolate PII |

### Layer 2 detection patterns

```
HIGH-CONFIDENCE (almost always real):
- "Password=" / "Pwd=" in a connection string OUTSIDE appsettings.Development.json / BackEnd/.env
- "Bearer " + base64 token literal in source
- "-----BEGIN PRIVATE KEY-----" / "-----BEGIN RSA PRIVATE KEY-----"
- Cloud provider key patterns (AWS "AKIA"+16 chars, Azure Storage/Service Bus keys)

MEDIUM-CONFIDENCE (needs context):
- ApiKey/Secret/Token variables with a string-literal assignment
- Base64 strings > 40 chars in source
- A connection string with a real server address outside dev config

FALSE POSITIVE — do not flag:
- appsettings.Development.json and BackEnd/.env values — both are gitignored by design
  (confirmed: `git check-ignore` both) — but never print their actual values in a report either
- Placeholders ("your-key-here", "changeme", empty strings)
- Test fixtures with obviously fake values
```

### Layer 3 patterns (adapt examples to this repo's actual code, don't invent hits)

- SQL injection: any `FromSqlRaw`/string-concatenated SQL with user input (none currently in the codebase — services use LINQ against `AppDbContext`; flag if a future change introduces raw SQL without parameterization).
- IDOR: an endpoint/service method taking an `id` without also filtering by the owning `userId` — see `DevicesController`/`DeviceService` for the correct pattern (`d.DeviceId == deviceId && d.UserId == userId`) and check new code against it.
- Crypto: MD5/SHA1 for anything security-relevant; `PasswordHash` should be a proper password hash (verify what's actually used before assuming bcrypt/PBKDF2/Argon2 — don't assume, check `RegisterUserDtoValidator`/wherever it's set).

### Layer 4 — when auth eventually exists

Checklist for that future work (not applicable today, keep for later): explicit `[Authorize]`/`[AllowAnonymous]` on every action, strict JWT validation (`ValidateIssuer`/`ValidateAudience`/`ValidateLifetime`/`ValidateIssuerSigningKey` all `true`, `ClockSkew` ≤ 1 minute), policy-based `[Authorize(Policy = "...")]` over bare `[Authorize]`, `UseAuthentication()` before `UseAuthorization()`.

### Severity and honesty rules

- This is **static analysis**, not a penetration test. Say so in every report.
- Match severity to real exploitability — a dev-only appsettings value is not HIGH; a missing `userId` filter on a `Device`/`Plant`/`Diagnosis` endpoint in this app is, because there's no other access control layer to catch it.
- **A finding needs location + concrete trigger + impact + fix**, not "could be an issue."
- Separate **confirmed** vulnerabilities (you read the code and traced the exploit path) from **suspected**/**recommended** (pattern match, not fully traced). Don't blur the two.
- **A scanner you didn't run is not evidence of "no vulnerabilities"** — say what you actually ran (`dotnet list package --vulnerable`, which files you greped) versus what you didn't.
- Do not mark a feature done/ready with a confirmed, in-scope Critical or High finding still open. If you can't fix it in scope, say so explicitly and why (needs a design decision, needs infra not available here, etc.) — never silently drop it from the report.

### Report format

```markdown
## Security Review

**Scope:** <files/feature reviewed> | **Method:** static analysis only (dotnet list package --vulnerable + source grep/read)

| Severity | Count |
|----------|-------|
| Critical | 0 |
| High     | 0 |
| Medium   | 0 |
| Low      | 0 |

#### [SEVERITY] file:line — Title (OWASP category)
Trigger: <input/sequence that hits it>
Impact: <what happens>
Fix: <specific change, before/after if code>
Status: confirmed | suspected

### Layer results
| Layer | Status | Findings |
|-------|--------|----------|
| 1. Packages | PASS/WARN/FAIL | ... |
| 2. Secrets | ... | ... |
| 3. OWASP patterns | ... | ... |
| 4. Auth (N/A today) | ... | ... |
| 5. CORS | ... | ... |
| 6. Data protection | ... | ... |
```
