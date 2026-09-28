# Workspace / farm / irrigation data model

Adds multi-tenant workspaces, farms, irrigation, device channels, automation, and satellite tracking to PlantPilot's data model. **Model, persistence, and migration only** — no new services/controllers/endpoints were added; existing ones (`DevicesController`/`DeviceService`) were left untouched and still compile and pass their tests unchanged.

## Why

PlantPal needs to support personal use (one user, their own plants) and business use (a farm operation with multiple people, fields, and automated irrigation) on the same schema, without breaking the existing personal-use flows.

## Entity map

```
Workspace (Personal|Business) ─┬─ WorkspaceMember (User × Role)
                                ├─ Plant (also keeps its original UserId — see "Backward compatibility")
                                ├─ Device (also keeps its original UserId)
                                ├─ Farm ─── Plot ─── (Plant.PlotId, optional)
                                │            └─ PlotObservation ─── SatelliteScene
                                │            └─ SatelliteCheck (per Farm)
                                └─ IrrigationZone (Plot optional, ValveChannel required)
                                     ├─ IrrigationZonePlant (× Plant)
                                     └─ IrrigationConfig (desired vs. device-confirmed)

Device ─── DeviceChannel (Sensor|Camera|ValveOutput|PumpOutput, with optional calibration)
             ├─ DeviceReading (measured/received timestamps, dedup index)
             ├─ DeviceCommand (idempotency key, expiry, optional Channel)
             └─ DeviceEvent (optional Channel/IrrigationZone/DeviceCommand, autonomous events allowed)

Alert ─── at most one of Plant / Device / IrrigationZone (real nullable FKs + CHECK, never a generic TargetId)
PhotoCaptureSchedule ─── Plant + Camera DeviceChannel
```

Diagnosis/Schedule/ScheduleTask/PlantPhoto kept their existing functionality and gained: `Diagnosis.HealthStatus` (Healthy/Unhealthy/**Indeterminate** — replaces the old boolean `IsHealthy`, which couldn't represent an indeterminate result) + `AnalysisStatus` + `AiModel`/`AiModelVersion`; `Schedule.DiagnosisId` is now nullable (a schedule can exist without a diagnosis); `ScheduleTask.Status` (Pending/Completed/**Cancelled** — replaces the old boolean `IsCompleted`) + `CancelledAt`; `PlantPhoto.Source` (Mobile/Camera) + optional `CameraChannelId` + separate `CapturedAt`/`UploadedAt`. No duplicate probability fields were found anywhere in the actual C# code (`Diagnosis.HealthProbability`, `DiagnosisProblem.Probability` each appear exactly once) — the diagram had duplicated rows, the code never did; nothing was removed here.

## Key decisions

- **Backward compatible, additive `WorkspaceId`**: `Plant.UserId`/`Device.UserId` were kept unchanged; `WorkspaceId` was added alongside them, not instead of them. This is why zero existing service/controller code needed to change — `DeviceService`/`DevicesController` still compile and work exactly as before. `WorkspaceId` is the model going forward; a future feature that builds real workspace-scoped endpoints should filter by it, and `UserId` becomes effectively "who added this," not the access-control boundary.
- **`PlantDevice` kept, not forced**: `Device`↔`Plant` association still exists via `PlantDevice` for whatever still wants a simple "which plants does this device serve" link, but `DeviceReading`/`DeviceCommand`/`DeviceEvent` no longer route through it — they reference `Device`/`DeviceChannel` directly, since a reading/command/event is fundamentally about a device (and specific channel), not about a device-plant pairing.
- **Geography via NetTopologySuite**: `Farm.Boundary`/`Plot.Boundary` are `NetTopologySuite.Geometries.Geometry` mapped to SQL Server's native `geography` column type (`Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite` 8.0.16, matching the other EF Core package versions already in the project) — this is the supported spatial mechanism for this project's actual database (SQL Server), not a generic/portable choice.
- **`Alert` links via real FKs, never a `TargetId`**: `PlantId`/`DeviceId`/`IrrigationZoneId` are all nullable FKs with an explicit CHECK constraint (`CK_Alert_SingleResource`) enforcing at most one is set (none set = workspace-level alert). Verified: the constraint rejects an insert with two resources set, accepts one.
- **`IrrigationConfig` desired vs. confirmed**: every tunable (thresholds, max duration, pause, max reading age, active flag) has an unprefixed "desired" (server intent) value and a nullable `Confirmed*` counterpart + `ConfirmedVersion`/`ConfirmedAt`, so the system can tell whether the device has actually applied the current config. `CK_IrrigationConfig_ThresholdOrder` enforces `StartThreshold < StopThreshold` — verified: rejects `(80, 20)`, accepts `(20, 80)`.
- **Cascade vs. Restrict**: SQL Server rejects a foreign key graph with more than one cascade path reaching the same table (this codebase already hit this twice before this change — see `AppDbContext.cs`'s existing comments on `Device→PlantDevices` and `Plant→Schedules`). This change introduces several new diamonds (e.g. `Workspace→Plant` direct vs. `Workspace→Farm→Plot→Plant`; `Device→DeviceCommand` direct vs. `Device→DeviceChannel→DeviceCommand`). Every one is resolved in favor of the more direct/primary-ownership path cascading and the secondary path set to `Restrict`, with an inline comment in `AppDbContext.cs` explaining which diamond each `Restrict` breaks — reviewed by `database-reviewer`. One of these diamonds was genuinely subtle: `Plant → Alert` looks independent of `Workspace` at first glance (`Plant.WorkspaceId` is already `Restrict`), but `Plot → Plant` is `SetNull` and `Workspace → Farm → Plot` is `Cascade`/`Cascade` — so a full cascading path `Workspace → Farm → Plot → Plant` already existed, and adding `Plant → Alert` as `SetNull` re-created the same conflict via that indirect route. Caught by actually running the migration (SQL Server error 1785), not by manual graph-tracing — `Plant → Alert` is `Restrict`; `Device → Alert` is `SetNull` (Device has no such indirect path, confirmed safe).
- **`PlantPhoto → DeviceChannel` (optional camera source)** now has the same explicit `Restrict` + comment as every other `DeviceChannel` relationship (`database-reviewer` finding — it was previously left to EF's implicit default, which happened to behave the same way but wasn't a reviewable, deliberate choice).
- **Geometry SRID**: `Farm.Boundary`/`Plot.Boundary` map to SQL Server `geography`, which requires SRID 4326 — plain NetTopologySuite geometries default to SRID 0 and nothing at the EF/column level can enforce the right SRID (`database-reviewer` finding, non-blocking since no Farm/Plot-creating code exists yet). Whoever builds that code must construct boundaries with a 4326-SRID factory (e.g. `NtsGeometryServices.Instance.CreateGeometryFactory(4326)`) or every insert will fail at runtime with SQL error 24205.

## Cross-workspace links: one closed by the DB, the rest still app-level

Independent review by `dotnet-security-reviewer` and `database-reviewer` (see their reports for the full analysis) found this gap in every join that crosses a `WorkspaceId` boundary. They disagreed on whether a cheap DB-level fix existed for the `IrrigationZonePlant` case — resolved by testing both claims directly rather than picking one:

- **`IrrigationZonePlant` (Plant ↔ IrrigationZone): now DB-enforced.** `IrrigationZonePlant.WorkspaceId` is denormalized onto the join row; `IrrigationZone`/`Plant` each got an alternate key `(Id, WorkspaceId)`; `IrrigationZonePlant` has two **composite** FKs — `(IrrigationZoneId, WorkspaceId) → IrrigationZone` and `(PlantId, WorkspaceId) → Plant`. Since `WorkspaceId` is one physical column on the join row, satisfying both FKs at once forces `IrrigationZone.WorkspaceId == Plant.WorkspaceId`. **Verified empirically, twice**: the exact cross-workspace insert that succeeded before this fix (linking a Workspace-B plant into a Workspace-A zone) is now rejected by the database with a real FK violation; the same insert with matching workspaces still succeeds. Migration: `Migrations/20260928092644_EnforceIrrigationZonePlantWorkspaceMatch.cs`.
- **Everything else is still app-level only, confirmed not cheaply DB-enforceable**: `IrrigationZone.ValveChannelId`/`IrrigationConfig.MoistureSensorChannelId` → `DeviceChannel.Device.WorkspaceId` (the channel has no `WorkspaceId` of its own to build the same composite-key trick on), `PhotoCaptureSchedule.CameraChannelId`/`PlantId` (same reason, plus `PhotoCaptureSchedule` has no `WorkspaceId` column at all today), `Alert.PlantId`/`DeviceId`/`IrrigationZoneId` vs. `Alert.WorkspaceId` (three independent optional resources, not a single one-hop join), and `Plant.PlotId` → `Plot.Farm.WorkspaceId` vs. `Plant.WorkspaceId`. **This must be enforced in application code** the moment a real service is built on any of these — flagged in `references/security.md` for `dotnet-reviewer`/`dotnet-security-reviewer` to check on every future feature that touches them.

## Migration and backfill

Two migrations. `Migrations/20260928090503_AddWorkspacesFarmsIrrigationAndDeviceChannels.cs` (the main schema/backfill) — EF Core's own scaffolding produced two real bugs that were hand-corrected before this was ever applied anywhere (see inline comments in the migration for the full explanation):

1. It renamed `DeviceEvents.PlantDeviceId` → `WaterAmountSource` (a type-match heuristic, not a real semantic rename) and `DeviceCommands.PlantDeviceId` → `DeviceId` (right name, wrong values — old `PlantDevice` ids left in place under the new column name). Both were fixed with a `Sql()` backfill that joins through `PlantDevices` to resolve the real `DeviceId` before the old values are overwritten/discarded.
2. `DeviceReading.ChannelId` is required, but `DeviceChannel` is a brand-new concept — no historical `DeviceReading` row has a deducible channel. Per the explicit "don't invent device associations that aren't deducible" rule, pre-existing rows are **archived** (not deleted, not guessed) to `DeviceReadings_LegacyArchive_PreChannelMigration` before the schema change.

**Personal-workspace backfill**: one `Workspace` (Type=Personal) + `WorkspaceMember` (Role=Owner) is created per existing `User` via a `MERGE ... OUTPUT` (the standard way to bulk-insert parents while capturing both the new and source keys in one statement), then every existing `Plant`/`Device` is pointed at its owning user's new workspace.

### Verified (not assumed) — tested against a seeded copy of the old schema in the local Docker SQL Server

- 2 users, 2 plants, 1 device, 1 `PlantDevice` link, 1 photo, 1 diagnosis (`IsHealthy=1`), 1 schedule, 2 schedule tasks (one completed, one pending), 1 device reading/command/event referencing the old `PlantDeviceId` — seeded before applying the migration.
- `ScheduleTask.Status`: completed task → `1` (Completed) with `CompletedAt` preserved; pending task → `0`. ✅
- `Diagnosis.HealthStatus`: `0` (Healthy), matching the seeded `IsHealthy=1`. ✅
- `PlantPhoto.CapturedAt = UploadedAt` for the pre-existing photo. ✅
- `DeviceCommand.DeviceId`/`DeviceEvent.DeviceId`: both resolved to the real `Device` row via the `PlantDevices` join, `DeviceEvent.WaterAmountSource` correctly reset to `0` (Unknown) afterward. ✅
- `DeviceReadings`: emptied; the one pre-existing row landed intact in `DeviceReadings_LegacyArchive_PreChannelMigration`. ✅
- `Workspaces`: exactly 2 personal workspaces created; `WorkspaceMembers`: 2 rows, `Role=0` (Owner), correctly correlated by user. ✅
- Every seeded `Plant`/`Device.WorkspaceId` matches its owning user's personal workspace (`R19` check: `COUNT(Workspaces WHERE Type=Personal) = COUNT(Users)`). ✅

Re-run this check on any database before trusting the backfill: `SELECT (SELECT COUNT(*) FROM Workspaces WHERE Type=0) AS PersonalWorkspaces, (SELECT COUNT(*) FROM Users) AS Users;` — the two counts must match, and `SELECT * FROM Plants p LEFT JOIN WorkspaceMembers wm ON wm.UserId = p.UserId AND wm.WorkspaceId = p.WorkspaceId WHERE wm.WorkspaceMemberId IS NULL;` must return zero rows.

The second migration, `Migrations/20260928092644_EnforceIrrigationZonePlantWorkspaceMatch.cs`, adds the `IrrigationZonePlant.WorkspaceId` composite-FK fix above. Its own backfill sets `WorkspaceId` from the linked `IrrigationZone`, then **deletes** any pre-existing row that turns out to violate the same-workspace invariant it introduces (there is no valid value to invent for a row that represents exactly the cross-workspace link this migration exists to prevent — verified against this repo's own test data, which had exactly one such row, created deliberately during review to prove the gap existed).

## Failure recovery

- **Rollback is schema-only, not data-safe — verified, not assumed.** `dotnet ef database update 20260928065140_InitialCreate` reverses the schema, but attempting this against the actual test database (after real writes had happened post-`Up()`) **failed with a live FK violation** — confirming a `database-reviewer` finding (HIGH) before it was fixed. The root cause: `Down()` originally tried to re-add `FK_.*_PlantDevices_PlantDeviceId` constraints against columns that, by rollback time, hold real `Device`/`DeviceChannel` ids (written after `Up()`), not the original `PlantDevice` ids `Up()` overwrote — so the FK-add either fails outright or silently mislinks a command/event to the wrong plant/device pairing. Fixed: `Down()` renames the columns back for schema symmetry but does **not** re-add those FK constraints, with a comment explaining why. Rollback now reverses the shape safely; it still does **not** restore the archived `DeviceReadings` rows (they stay in `DeviceReadings_LegacyArchive_PreChannelMigration`) or the semantic backfills (`Status`/`HealthStatus`/`CapturedAt` — those columns are simply dropped by `Down()`).
- **Roll-forward vs. restore**: if either migration is ever found to have a bug in a real deployment with real data, the correct response is a new forward-fixing migration (see `references/migrations.md`) — not a rollback-and-reapply, since the archived/backfilled data's original source columns will already be gone after `Down()`.
- **Backup**: not verified/assumed to exist for this local dev environment — there is none, and none is claimed. For any real deployment, verify an actual recent backup exists before relying on "restore" as a recovery option.

## Diagram

`DataBasePlantPal.drawio` (in the user's Downloads folder, **outside this repository**) was not hand-edited as part of this change — manually authoring ~20 new mxGraph table blocks with correct geometry in raw XML has a high error/corruption risk relative to this document, which is the authoritative, versioned description of the current schema. If the `.drawio` file should stay in sync, regenerating it from this document (or from the live schema) is a separate, explicitly-scoped follow-up.
