using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Models;
using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Diagnoses;
using PlantCare.Api.Models.Schedules;
using PlantCare.Api.Models.Devices;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Models.Farms;
using PlantCare.Api.Models.Irrigation;
using PlantCare.Api.Models.Automation;
using PlantCare.Api.Models.Satellite;
using PlantCare.Api.Models.Auth;

namespace PlantCare.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<PlantPhoto> PlantPhotos => Set<PlantPhoto>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<DiagnosisProblem> DiagnosisProblems => Set<DiagnosisProblem>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduleTask> ScheduleTasks => Set<ScheduleTask>();

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceChannel> DeviceChannels => Set<DeviceChannel>();
    public DbSet<PlantDevice> PlantDevices => Set<PlantDevice>();
    public DbSet<DeviceCommand> DeviceCommands => Set<DeviceCommand>();
    public DbSet<DeviceEvent> DeviceEvents => Set<DeviceEvent>();
    public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Plot> Plots => Set<Plot>();

    public DbSet<IrrigationZone> IrrigationZones => Set<IrrigationZone>();
    public DbSet<IrrigationZonePlant> IrrigationZonePlants => Set<IrrigationZonePlant>();
    public DbSet<IrrigationConfig> IrrigationConfigs => Set<IrrigationConfig>();

    public DbSet<PhotoCaptureSchedule> PhotoCaptureSchedules => Set<PhotoCaptureSchedule>();
    public DbSet<Alert> Alerts => Set<Alert>();

    public DbSet<SatelliteScene> SatelliteScenes => Set<SatelliteScene>();
    public DbSet<PlotObservation> PlotObservations => Set<PlotObservation>();
    public DbSet<SatelliteCheck> SatelliteChecks => Set<SatelliteCheck>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Explicit PKs: property names don't follow the {EntityName}Id/Id convention
        modelBuilder.Entity<DiagnosisProblem>().HasKey(p => p.ProblemId);
        modelBuilder.Entity<ScheduleTask>().HasKey(t => t.TaskId);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired();

        // User 1 -> Many Plants
        modelBuilder.Entity<User>()
            .HasMany(u => u.Plants)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User 1 -> Many Devices
        modelBuilder.Entity<User>()
            .HasMany<Device>()
            .WithOne()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plant 1 -> Many Photos
        modelBuilder.Entity<Plant>()
            .HasMany(p => p.Photos)
            .WithOne(ph => ph.Plant)
            .HasForeignKey(ph => ph.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlantPhoto 1 -> 0..1 Diagnosis
        modelBuilder.Entity<PlantPhoto>()
            .HasOne(ph => ph.Diagnosis)
            .WithOne(d => d.PlantPhoto)
            .HasForeignKey<Diagnosis>(d => d.PlantPhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Diagnosis>()
            .HasIndex(d => d.PlantPhotoId)
            .IsUnique();

        // Diagnosis 1 -> Many Problems
        modelBuilder.Entity<Diagnosis>()
            .HasMany(d => d.Problems)
            .WithOne(p => p.Diagnosis)
            .HasForeignKey(p => p.DiagnosisId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plant 1 -> Many Schedules
        // Restrict (not Cascade) to avoid multiple cascade paths to Schedules via
        // Plant -> Schedules directly and Plant -> PlantPhotos -> Diagnoses -> Schedule, which SQL Server
        // rejects. The Diagnosis chain already cascades the delete in practice.
        modelBuilder.Entity<Plant>()
            .HasMany(p => p.Schedules)
            .WithOne(s => s.Plant)
            .HasForeignKey(s => s.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Diagnosis 1 -> 1 Schedule
        // Schedule.DiagnosisId is a non-nullable int, so this relationship is required (SetNull is
        // not valid on a non-nullable FK column); cascade matches the rest of the Photo->Diagnosis chain.
        modelBuilder.Entity<Diagnosis>()
            .HasOne(d => d.Schedule)
            .WithOne(s => s.Diagnosis)
            .HasForeignKey<Schedule>(s => s.DiagnosisId)
            .OnDelete(DeleteBehavior.Cascade);

        // Schedule 1 -> Many Tasks
        modelBuilder.Entity<Schedule>()
            .HasMany(s => s.Tasks)
            .WithOne(t => t.Schedule)
            .HasForeignKey(t => t.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Device 1 -> Many PlantDevices
        // Restrict (not Cascade) to avoid multiple cascade paths to PlantDevices via
        // Users -> Devices -> PlantDevices and Users -> Plants -> PlantDevices, which SQL Server rejects.
        modelBuilder.Entity<Device>()
            .HasMany(d => d.PlantDevices)
            .WithOne(pd => pd.Device)
            .HasForeignKey(pd => pd.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Plant 1 -> Many PlantDevices
        modelBuilder.Entity<Plant>()
            .HasMany(p => p.PlantDevices)
            .WithOne(pd => pd.Plant)
            .HasForeignKey(pd => pd.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Avoid duplicate device-plant links
        modelBuilder.Entity<PlantDevice>()
            .HasIndex(pd => new { pd.DeviceId, pd.PlantId })
            .IsUnique();

        // Percentage precision (0-100.00)
        modelBuilder.Entity<Diagnosis>()
            .Property(d => d.HealthProbability)
            .HasPrecision(5, 2);

        modelBuilder.Entity<DiagnosisProblem>()
            .Property(p => p.Probability)
            .HasPrecision(5, 2);

        // ApiKeyHash required
        modelBuilder.Entity<Device>()
            .Property(d => d.ApiKeyHash)
            .IsRequired();

        // Battery level default
        modelBuilder.Entity<Device>()
            .Property(d => d.BatteryLevel)
            .HasDefaultValue(100);

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.SerialNumber)
            .IsUnique();

        // ===== Workspaces =====

        modelBuilder.Entity<WorkspaceMember>()
            .HasIndex(m => new { m.WorkspaceId, m.UserId })
            .IsUnique();

        modelBuilder.Entity<Workspace>()
            .HasMany(w => w.Members)
            .WithOne(m => m.Workspace)
            .HasForeignKey(m => m.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkspaceMember -> User: Restrict, not Cascade — avoid a second cascade path to
        // WorkspaceMember alongside Workspace -> WorkspaceMember (both reachable if a shared
        // ancestor existed); also keeps a user's membership history intact if the user row itself
        // is ever removed independently of workspace cleanup.
        modelBuilder.Entity<WorkspaceMember>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Plant/Device -> Workspace: Restrict. Plant/Device already cascade from User directly;
        // Workspace ownership is the new, additive scoping column (see Plant.UserId/Device.UserId,
        // kept for backward compatibility) — deleting a Workspace must not silently cascade into
        // deleting Plants/Devices that are also still reachable via their User.
        modelBuilder.Entity<Plant>()
            .HasOne(p => p.Workspace)
            .WithMany()
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Device>()
            .HasOne(d => d.Workspace)
            .WithMany()
            .HasForeignKey(d => d.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Farms =====

        modelBuilder.Entity<Farm>()
            .Property(f => f.Boundary)
            .HasColumnType("geography");

        modelBuilder.Entity<Plot>()
            .Property(p => p.Boundary)
            .HasColumnType("geography");

        modelBuilder.Entity<Farm>()
            .HasOne(f => f.Workspace)
            .WithMany()
            .HasForeignKey(f => f.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Farm>()
            .HasMany(f => f.Plots)
            .WithOne(p => p.Farm)
            .HasForeignKey(p => p.FarmId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plant -> Plot: SetNull, not Cascade — Plot is optional on Plant, and Plot is also
        // reachable from Workspace via Farm -> Plot; a direct Cascade here plus the direct
        // Workspace -> Plant Restrict above keeps this a single, unambiguous path (deleting a
        // Plot un-assigns its plants instead of deleting them).
        modelBuilder.Entity<Plant>()
            .HasOne(p => p.Plot)
            .WithMany()
            .HasForeignKey(p => p.PlotId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== Device channels =====

        modelBuilder.Entity<Device>()
            .HasMany(d => d.Channels)
            .WithOne(c => c.Device)
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // DeviceReading only ever reaches Device through Channel (no direct DeviceId), so this is
        // the single cascade path — safe.
        modelBuilder.Entity<DeviceChannel>()
            .HasMany<DeviceReading>()
            .WithOne(r => r.Channel)
            .HasForeignKey(r => r.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceReading>()
            .HasIndex(r => new { r.ChannelId, r.ReadingType, r.MeasuredAt })
            .IsUnique();

        // DeviceCommand/DeviceEvent both have a direct, required Device FK (the primary cascade
        // path) plus an optional Channel FK — the Channel FK must be Restrict to avoid a second
        // cascade path to the same table from Device (Device -> Channel -> Command/Event).
        modelBuilder.Entity<Device>()
            .HasMany<DeviceCommand>()
            .WithOne(c => c.Device)
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceChannel>()
            .HasMany<DeviceCommand>()
            .WithOne(c => c.Channel)
            .HasForeignKey(c => c.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeviceCommand>()
            .HasIndex(c => c.IdempotencyKey)
            .IsUnique();

        modelBuilder.Entity<Device>()
            .HasMany<DeviceEvent>()
            .WithOne(e => e.Device)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceChannel>()
            .HasMany<DeviceEvent>()
            .WithOne(e => e.Channel)
            .HasForeignKey(e => e.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        // DeviceCommand -> DeviceEvent (optional "triggered by this command" link): Restrict —
        // DeviceCommand is itself owned by Device (cascade above), so this would otherwise be a
        // second Device -> DeviceCommand -> DeviceEvent path alongside the direct one.
        modelBuilder.Entity<DeviceCommand>()
            .HasMany<DeviceEvent>()
            .WithOne(e => e.DeviceCommand)
            .HasForeignKey(e => e.DeviceCommandId)
            .OnDelete(DeleteBehavior.Restrict);

        // PlantPhoto -> DeviceChannel (optional camera source): explicit Restrict, matching every
        // other DeviceChannel relationship in this file — without this, EF falls back to its
        // default (ClientSetNull/NO ACTION) for an optional FK, which behaves the same today but
        // was an implicit convention fallback rather than a deliberate, reviewable choice.
        modelBuilder.Entity<DeviceChannel>()
            .HasMany<PlantPhoto>()
            .WithOne(p => p.CameraChannel)
            .HasForeignKey(p => p.CameraChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Irrigation =====

        modelBuilder.Entity<IrrigationZone>()
            .HasOne(z => z.Workspace)
            .WithMany()
            .HasForeignKey(z => z.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plot -> IrrigationZone: Restrict — Zone is also directly Workspace-owned (cascade
        // above); Plot is reachable from the same Workspace via Farm, so this must not also
        // cascade (a zone survives its optional plot being deleted; PlotId is nullable anyway).
        modelBuilder.Entity<Plot>()
            .HasMany<IrrigationZone>()
            .WithOne(z => z.Plot)
            .HasForeignKey(z => z.PlotId)
            .OnDelete(DeleteBehavior.Restrict);

        // A Zone requires a valve channel to exist; deleting the channel must not silently delete
        // the zone — reassign or delete the zone first.
        modelBuilder.Entity<DeviceChannel>()
            .HasMany<IrrigationZone>()
            .WithOne(z => z.ValveChannel)
            .HasForeignKey(z => z.ValveChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        // IrrigationZonePlant carries a denormalized WorkspaceId and both FKs below are composite
        // (Id, WorkspaceId) against an alternate key on the parent, instead of the usual single-
        // column FK. Since WorkspaceId is one physical column on the join row, satisfying both
        // composite FKs at once forces IrrigationZone.WorkspaceId == Plant.WorkspaceId — the DB
        // rejects linking a plant and a zone from different workspaces (security-reviewer finding,
        // verified empirically — see docs/architecture/workspace-data-model.md). The alternate
        // keys are trivially satisfiable (each entity's own Id is already unique on its own).
        modelBuilder.Entity<IrrigationZone>()
            .HasAlternateKey(z => new { z.IrrigationZoneId, z.WorkspaceId });

        modelBuilder.Entity<Plant>()
            .HasAlternateKey(p => new { p.PlantId, p.WorkspaceId });

        modelBuilder.Entity<IrrigationZone>()
            .HasMany(z => z.Plants)
            .WithOne(zp => zp.IrrigationZone)
            .HasForeignKey(zp => new { zp.IrrigationZoneId, zp.WorkspaceId })
            .HasPrincipalKey(z => new { z.IrrigationZoneId, z.WorkspaceId })
            .OnDelete(DeleteBehavior.Cascade);

        // Plant -> IrrigationZonePlant: Restrict — Zone already cascades to IrrigationZonePlant
        // above, and Plant is reachable from the same Workspace independently; only one side may
        // cascade to this join table.
        modelBuilder.Entity<Plant>()
            .HasMany<IrrigationZonePlant>()
            .WithOne(zp => zp.Plant)
            .HasForeignKey(zp => new { zp.PlantId, zp.WorkspaceId })
            .HasPrincipalKey(p => new { p.PlantId, p.WorkspaceId })
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<IrrigationZonePlant>()
            .HasIndex(zp => new { zp.IrrigationZoneId, zp.PlantId })
            .IsUnique();

        modelBuilder.Entity<IrrigationZone>()
            .HasOne(z => z.Config)
            .WithOne(c => c.IrrigationZone)
            .HasForeignKey<IrrigationConfig>(c => c.IrrigationZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceChannel>()
            .HasMany<IrrigationConfig>()
            .WithOne(c => c.MoistureSensorChannel)
            .HasForeignKey(c => c.MoistureSensorChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<IrrigationConfig>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_IrrigationConfig_ThresholdOrder",
                "[StartThreshold] < [StopThreshold]"));

        // DeviceEvent -> IrrigationZone: Restrict is a deliberate choice (not SQL-Server-forced —
        // no multi-path conflict traces through this one), so a zone's historical event log
        // survives even if the zone itself is later deleted, rather than vanishing with it.
        modelBuilder.Entity<DeviceEvent>()
            .HasOne(e => e.IrrigationZone)
            .WithMany()
            .HasForeignKey(e => e.IrrigationZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Automation =====

        modelBuilder.Entity<Plant>()
            .HasMany<PhotoCaptureSchedule>()
            .WithOne(s => s.Plant)
            .HasForeignKey(s => s.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceChannel>()
            .HasMany<PhotoCaptureSchedule>()
            .WithOne(s => s.CameraChannel)
            .HasForeignKey(s => s.CameraChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Workspace>()
            .HasMany<Alert>()
            .WithOne(a => a.Workspace)
            .HasForeignKey(a => a.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Device -> Alert: SetNull, not Restrict. Device is Restrict from Workspace directly and
        // has no other cascading path reaching it, so there is no SQL-Server cascade-path conflict
        // — this is a deliberate UX choice: preserve alert history and un-link it, rather than
        // blocking deletion of a Device while an old alert about it still exists (database-reviewer
        // finding; matches the precedent set by Plot -> Plant below).
        modelBuilder.Entity<Device>()
            .HasMany<Alert>()
            .WithOne(a => a.Device)
            .HasForeignKey(a => a.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Plant -> Alert: Restrict, NOT SetNull, unlike Device above — this one IS SQL-Server-
        // forced (verified empirically: SetNull here was rejected with error 1785). There is
        // already a full cascading path Workspace -> Farm -> Plot -> Plant (Cascade, Cascade,
        // SetNull — see the Farms section above), so adding a second cascading action
        // (Plant -> Alert) creates the same "multiple cascade paths from Workspace" conflict this
        // file has hit several times before. Restrict here means an Alert about a Plant must be
        // resolved/deleted before the Plant can be deleted.
        modelBuilder.Entity<Plant>()
            .HasMany<Alert>()
            .WithOne(a => a.Plant)
            .HasForeignKey(a => a.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        // IrrigationZone -> Alert: Restrict IS structurally forced here (unlike Plant/Device above)
        // — Workspace -> IrrigationZone is Cascade, so Workspace -> Alert direct vs.
        // Workspace -> IrrigationZone -> Alert would be a real multi-cascade-path conflict if this
        // were Cascade or SetNull (SQL Server treats both as "referential actions" for this check).
        modelBuilder.Entity<IrrigationZone>()
            .HasMany<Alert>()
            .WithOne(a => a.IrrigationZone)
            .HasForeignKey(a => a.IrrigationZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        // At most one resource FK set (or none, for a workspace-level alert) — never a generic,
        // unconstrained TargetId.
        modelBuilder.Entity<Alert>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Alert_SingleResource",
                "(CASE WHEN [PlantId] IS NOT NULL THEN 1 ELSE 0 END " +
                "+ CASE WHEN [DeviceId] IS NOT NULL THEN 1 ELSE 0 END " +
                "+ CASE WHEN [IrrigationZoneId] IS NOT NULL THEN 1 ELSE 0 END) <= 1"));

        // ===== Satellite =====

        modelBuilder.Entity<SatelliteScene>()
            .HasIndex(s => new { s.Provider, s.ExternalId })
            .IsUnique();

        modelBuilder.Entity<Plot>()
            .HasMany<PlotObservation>()
            .WithOne(o => o.Plot)
            .HasForeignKey(o => o.PlotId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SatelliteScene>()
            .HasMany(s => s.Observations)
            .WithOne(o => o.SatelliteScene)
            .HasForeignKey(o => o.SatelliteSceneId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlotObservation>()
            .HasIndex(o => new { o.PlotId, o.SatelliteSceneId })
            .IsUnique();

        modelBuilder.Entity<Farm>()
            .HasMany<SatelliteCheck>()
            .WithOne(c => c.Farm)
            .HasForeignKey(c => c.FarmId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Auth =====

        // RefreshToken -> User: Cascade. Unlike WorkspaceMember (Restrict, to preserve membership
        // history), a refresh token has no meaning without its user and isn't a user-facing history
        // record — deleting a user should delete their refresh tokens too, no orphan-history concern.
        modelBuilder.Entity<RefreshToken>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique so a hash collision/reuse is detectable at the DB level too, and so the
        // TokenHash lookup RefreshTokenService relies on is always unambiguous.
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique();

        // Speeds "revoke every active token for this user" queries (reuse-detection and logout).
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(t => t.UserId);

        // RefreshToken -> RefreshToken (self-referencing rotation chain): Restrict, not Cascade — a
        // self-referencing FK can't validly cascade in SQL Server for this kind of chain, and Restrict
        // is also the semantically correct choice: deleting a RefreshToken row should never be allowed
        // to silently delete or null out a different row's pointer to it. EF Core's default FK
        // convention also gives ReplacedByRefreshTokenId an index, which speeds walking the chain.
        modelBuilder.Entity<RefreshToken>()
            .HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(t => t.ReplacedByRefreshTokenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}