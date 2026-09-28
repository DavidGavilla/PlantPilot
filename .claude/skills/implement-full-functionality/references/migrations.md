# EF Core migrations — review and safe-apply workflow

Adapted from `codewithmukesh/dotnet-claude-kit`, skill `migrate`, **Flow A (EF Core schema) only** — Flow B (.NET version upgrade) and Flow C (NuGet updates) were not pulled in, since this task is scoped to schema migrations and the project isn't upgrading .NET or bulk-updating packages (MIT License, © 2025 Mukesh Murugan, https://github.com/codewithmukesh/dotnet-claude-kit). **Changed for this repo**: the source assumes `--project <Infra> --startup-project <Api>` (a layered solution) and Postgres double-quoted identifiers — this repo has **no separate Infrastructure project** (`AppDbContext` and all entities live directly in `PlantCare.Api`), so every command below drops those flags and runs from `BackEnd/PlantCare.Api/` directly, using the local `dotnet-ef` tool (`.config/dotnet-tools.json`, pinned to 8.0.16) against **SQL Server** (bracket `[Identifier]` syntax, not Postgres double quotes). The source also uses MCP tools (`find_symbol`, `get_type_hierarchy`, `find_references`, `get_diagnostics`) that aren't configured in this session — replaced with `Grep`/`Read` below.

This extends, not replaces, the migration rules already in `dotnet-implementer.md` and `database-reviewer.md` (PK convention, cardinality binding, cascade-path limits, `SetNull` needing a nullable FK, decimal precision) — those are this repo's own hard-won lessons; this file adds the **process** around generating/reviewing/applying/rolling back.

## Step 1 — Assess current state

```bash
cd BackEnd/PlantCare.Api
dotnet ef migrations list
```

Check whether there's already a pending model change not yet captured in a migration, and whether the target database (local Docker SQL Server, `BackEnd/docker-compose.yml`) is up first (`docker compose up -d` from `BackEnd/`).

## Step 2 — Review the model change

Before generating anything, `Grep`/`Read` the entity class(es) and `AppDbContext.OnModelCreating` for the change. Confirm it's **one logical unit** — if a change bundles two unrelated schema changes, split it into two migrations; a mixed migration makes rollback all-or-nothing.

## Step 3 — Generate the migration

Name describes the change, not the entity:

```bash
# GOOD
dotnet ef migrations add AddDeviceCommandExpiry
# BAD — names the entity, not the change
dotnet ef migrations add DeviceCommand
```

## Step 4 — Review the generated SQL and the migration file itself

`dotnet ef database update` has no dry-run. Preview with an idempotent script:

```bash
dotnet ef migrations script --idempotent -o /tmp/migration-preview.sql
```

Read the migration `.cs` file (not just the SQL) and flag:

- **`DropColumn`/`DropTable`** — confirm data loss is intentional and expected.
- **`AlterColumn`** type/precision changes — check for truncation (this repo has already hit missing-`HasPrecision` truncation once).
- **A new non-nullable column on a table that can have existing rows** — needs a default, or a two-step add-nullable-then-backfill-then-require.
- **Any warning EF Core printed during `migrations add`** (shadow properties, cascade-path rejections) — read `dotnet-implementer.md`/`database-reviewer.md` for the exact failure modes this repo has already hit; don't apply a migration that was generated with an unresolved warning.
- **Large-table `ALTER`** — flag potential lock duration if the table could hold meaningful data in a real deployment (not a concern yet for a freshly-migrated dev DB, but note it so it isn't forgotten later).

### Renaming/retyping without losing data

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>("ContactEmail", "Users", nullable: true);
    migrationBuilder.Sql("UPDATE [Users] SET [ContactEmail] = [Email]"); // SQL Server bracket syntax
    migrationBuilder.AlterColumn<string>("ContactEmail", "Users", nullable: false);
    migrationBuilder.DropColumn("Email", "Users");
}
```

Plan a backfill (`Sql(...)` call inside the migration, or a documented follow-up step) whenever a change would otherwise silently drop existing values — even though this project's local dev DB currently has no real data to lose, write migrations as if it did, since that's the artifact that ships.

## Step 5 — Apply and verify

```bash
dotnet ef database update
cd ../.. && cd BackEnd && dotnet build PlantCare.Api && dotnet test PlantCare.Api.Tests
```

If applying fails (as it has, twice, for this exact schema — multi-cascade-path FK, `SetNull` on a non-nullable column): **fix `OnModelCreating`**, then delete the just-generated, never-successfully-applied migration's files and regenerate — never leave two migrations where the second "patches" a first one that was never applied anywhere (see `dotnet-implementer.md`'s existing rule).

## Step 6 — Document the rollback

```bash
dotnet ef database update <PreviousMigrationName>   # roll the DB back
dotnet ef migrations remove                          # only if also unapplying from code, and only if never applied elsewhere
```

**Never modify a migration that is already applied anywhere** (including just the local dev DB, once someone else might have applied it too) — create a new migration instead. **Never assume `Down()` recovers dropped data** — an EF Core `Down()` reverses the *schema* change; it does not restore rows a `DROP COLUMN`/`DROP TABLE` in `Up()` deleted, unless the migration explicitly backed them up first. State this limitation explicitly if a rollback plan depends on data recovery.

## Production and backup discipline (Section 9, applies here)

- Never apply a migration to production as part of this workflow — everything above targets the local Docker SQL Server only.
- Never assume a backup exists — if a real deployment's rollback plan depends on "restore from backup," verify the backup exists and is recent before relying on it in the plan; don't state it as available without checking.
- Never run a "recovery" operation (restore, rollback-and-reapply) against production while testing a migration — test the rollback path against the local/dev database.
- Decide, per change, whether recovery from a bad migration in a real deployment would be **rollback** (revert schema + code together), **roll-forward** (ship a fixing migration), or **restore** (only if data loss already happened) — and say which, rather than leaving it implicit.

## Flow B/C (not adapted here)

If a future task needs a `.NET` version upgrade or a bulk NuGet update workflow, pull `dotnet-upgrade`/`dotnet-nuget` from the already-installed `dotnet/skills` marketplace first (see `CLAUDE.md`) — they cover version-jump migrations and Central Package Management, respectively, and are already available in this project without needing another external import.
