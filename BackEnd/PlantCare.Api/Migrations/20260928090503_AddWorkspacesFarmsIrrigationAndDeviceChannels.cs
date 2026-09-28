using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PlantCare.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspacesFarmsIrrigationAndDeviceChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceCommands_PlantDevices_PlantDeviceId",
                table: "DeviceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceEvents_PlantDevices_PlantDeviceId",
                table: "DeviceEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceReadings_PlantDevices_PlantDeviceId",
                table: "DeviceReadings");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_DiagnosisId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_PlantDeviceId",
                table: "DeviceReadings");

            migrationBuilder.DropIndex(
                name: "IX_DeviceEvents_PlantDeviceId",
                table: "DeviceEvents");

            // DeviceReading.ChannelId is required (every reading must come from a specific
            // channel), but DeviceChannel is a brand-new concept — no historical DeviceReading row
            // has a deducible channel (its old PlantDeviceId identifies a device+plant pairing,
            // not which sensor/camera/output on that device produced the reading). Per "no
            // inventar asociaciones de dispositivos si los datos antiguos no permiten
            // deducirlas", this migration does not guess. Historical rows are archived (not
            // deleted) so the data survives, verifiable via
            // SELECT COUNT(*) FROM DeviceReadings_LegacyArchive_PreChannelMigration.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM [DeviceReadings])
                BEGIN
                    SELECT * INTO [DeviceReadings_LegacyArchive_PreChannelMigration] FROM [DeviceReadings];
                    DELETE FROM [DeviceReadings];
                END
            ");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "PlantPhotos",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "PlantDeviceId",
                table: "DeviceReadings",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "DeviceReadings",
                newName: "ReceivedAt");

            migrationBuilder.RenameColumn(
                name: "PlantDeviceId",
                table: "DeviceEvents",
                newName: "WaterAmountSource");

            migrationBuilder.RenameColumn(
                name: "PlantDeviceId",
                table: "DeviceCommands",
                newName: "DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceCommands_PlantDeviceId",
                table: "DeviceCommands",
                newName: "IX_DeviceCommands_DeviceId");

            // Backfill: the rename above preserved the old PlantDeviceId VALUES under the new
            // DeviceId NAME — they are not real Device ids yet. Derive the real DeviceId via the
            // PlantDevices join (deducible, unlike Channel-level detail).
            migrationBuilder.Sql(@"
                UPDATE dc
                SET dc.[DeviceId] = pd.[DeviceId]
                FROM [DeviceCommands] dc
                INNER JOIN [PlantDevices] pd ON pd.[PlantDeviceId] = dc.[DeviceId];
            ");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "ScheduleTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ScheduleTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill: ScheduleTaskStatus.Completed = 1. Pending (0, the column default) already
            // covers every row where IsCompleted was false.
            migrationBuilder.Sql("UPDATE [ScheduleTasks] SET [Status] = 1 WHERE [IsCompleted] = 1;");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "ScheduleTasks");

            migrationBuilder.AlterColumn<int>(
                name: "DiagnosisId",
                table: "Schedules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PlotId",
                table: "Plants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Plants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CameraChannelId",
                table: "PlantPhotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CapturedAt",
                table: "PlantPhotos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Backfill: before this change there was one timestamp (now UploadedAt, via the
            // rename above); treat it as the capture time too for pre-existing photos.
            migrationBuilder.Sql("UPDATE [PlantPhotos] SET [CapturedAt] = [UploadedAt];");

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "PlantPhotos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AiModel",
                table: "Diagnoses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiModelVersion",
                table: "Diagnoses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnalysisStatus",
                table: "Diagnoses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HealthStatus",
                table: "Diagnoses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill: DiagnosisHealthStatus.Unhealthy = 1. Healthy (0, the column default)
            // already covers every row where IsHealthy was true. No historical row can map to
            // Indeterminate (2) — that outcome didn't exist before this change.
            migrationBuilder.Sql("UPDATE [Diagnoses] SET [HealthStatus] = 1 WHERE [IsHealthy] = 0;");

            migrationBuilder.DropColumn(
                name: "IsHealthy",
                table: "Diagnoses");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "Devices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Devices",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "MeasuredAt",
                table: "DeviceReadings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Backfill: before this change there was one timestamp (now ReceivedAt, via the
            // rename above); treat it as the measurement time too for pre-existing readings.
            migrationBuilder.Sql("UPDATE [DeviceReadings] SET [MeasuredAt] = [ReceivedAt];");

            // NOTE: pre-existing rows (if any) were already archived to
            // DeviceReadings_LegacyArchive_PreChannelMigration above, precisely because ChannelId
            // has no deducible historical value — see that comment for the full rationale. The
            // table is empty by this point, so this rename is a pure structural change now.

            migrationBuilder.AddColumn<int>(
                name: "ChannelId",
                table: "DeviceEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceCommandId",
                table: "DeviceEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "DeviceEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill: the old "PlantDeviceId" column on DeviceEvents was renamed to
            // "WaterAmountSource" above (a scaffolding heuristic match on column type, not a real
            // semantic rename) — at this point it still holds the OLD PlantDeviceId integer
            // values. Derive the real DeviceId via the PlantDevices join (this part IS deducible,
            // unlike Channel) before overwriting it with a real WaterAmountSource default.
            migrationBuilder.Sql(@"
                UPDATE de
                SET de.[DeviceId] = pd.[DeviceId]
                FROM [DeviceEvents] de
                INNER JOIN [PlantDevices] pd ON pd.[PlantDeviceId] = de.[WaterAmountSource];

                UPDATE [DeviceEvents] SET [WaterAmountSource] = 0;
            ");

            migrationBuilder.AddColumn<int>(
                name: "IrrigationZoneId",
                table: "DeviceEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChannelId",
                table: "DeviceCommands",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "DeviceCommands",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "DeviceCommands",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceChannels",
                columns: table => new
                {
                    DeviceChannelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    ChannelType = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CalibrationOffset = table.Column<double>(type: "float", nullable: true),
                    CalibrationScale = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceChannels", x => x.DeviceChannelId);
                    table.ForeignKey(
                        name: "FK_DeviceChannels_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatelliteScenes",
                columns: table => new
                {
                    SatelliteSceneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolutionMeters = table.Column<double>(type: "float", nullable: false),
                    FileReference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatelliteScenes", x => x.SatelliteSceneId);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    WorkspaceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.WorkspaceId);
                });

            migrationBuilder.CreateTable(
                name: "PhotoCaptureSchedules",
                columns: table => new
                {
                    PhotoCaptureScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CameraChannelId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoCaptureSchedules", x => x.PhotoCaptureScheduleId);
                    table.ForeignKey(
                        name: "FK_PhotoCaptureSchedules_DeviceChannels_CameraChannelId",
                        column: x => x.CameraChannelId,
                        principalTable: "DeviceChannels",
                        principalColumn: "DeviceChannelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhotoCaptureSchedules_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "PlantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    FarmId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Boundary = table.Column<Geometry>(type: "geography", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farms", x => x.FarmId);
                    table.ForeignKey(
                        name: "FK_Farms_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceMembers",
                columns: table => new
                {
                    WorkspaceMemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceMembers", x => x.WorkspaceMemberId);
                    table.ForeignKey(
                        name: "FK_WorkspaceMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkspaceMembers_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plots",
                columns: table => new
                {
                    PlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Boundary = table.Column<Geometry>(type: "geography", nullable: true),
                    CropType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plots", x => x.PlotId);
                    table.ForeignKey(
                        name: "FK_Plots_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "FarmId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatelliteChecks",
                columns: table => new
                {
                    SatelliteCheckId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatelliteChecks", x => x.SatelliteCheckId);
                    table.ForeignKey(
                        name: "FK_SatelliteChecks_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "FarmId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IrrigationZones",
                columns: table => new
                {
                    IrrigationZoneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlotId = table.Column<int>(type: "int", nullable: true),
                    ValveChannelId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrrigationZones", x => x.IrrigationZoneId);
                    table.ForeignKey(
                        name: "FK_IrrigationZones_DeviceChannels_ValveChannelId",
                        column: x => x.ValveChannelId,
                        principalTable: "DeviceChannels",
                        principalColumn: "DeviceChannelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IrrigationZones_Plots_PlotId",
                        column: x => x.PlotId,
                        principalTable: "Plots",
                        principalColumn: "PlotId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IrrigationZones_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlotObservations",
                columns: table => new
                {
                    PlotObservationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlotId = table.Column<int>(type: "int", nullable: false),
                    SatelliteSceneId = table.Column<int>(type: "int", nullable: false),
                    Ndvi = table.Column<double>(type: "float", nullable: true),
                    ValidCoveragePercent = table.Column<double>(type: "float", nullable: false),
                    Quality = table.Column<int>(type: "int", nullable: false),
                    ProcessingVersion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlotObservations", x => x.PlotObservationId);
                    table.ForeignKey(
                        name: "FK_PlotObservations_Plots_PlotId",
                        column: x => x.PlotId,
                        principalTable: "Plots",
                        principalColumn: "PlotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlotObservations_SatelliteScenes_SatelliteSceneId",
                        column: x => x.SatelliteSceneId,
                        principalTable: "SatelliteScenes",
                        principalColumn: "SatelliteSceneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    AlertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<int>(type: "int", nullable: true),
                    IrrigationZoneId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.AlertId);
                    table.CheckConstraint("CK_Alert_SingleResource", "(CASE WHEN [PlantId] IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN [DeviceId] IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN [IrrigationZoneId] IS NOT NULL THEN 1 ELSE 0 END) <= 1");
                    table.ForeignKey(
                        name: "FK_Alerts_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_IrrigationZones_IrrigationZoneId",
                        column: x => x.IrrigationZoneId,
                        principalTable: "IrrigationZones",
                        principalColumn: "IrrigationZoneId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "PlantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IrrigationConfigs",
                columns: table => new
                {
                    IrrigationConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IrrigationZoneId = table.Column<int>(type: "int", nullable: false),
                    MoistureSensorChannelId = table.Column<int>(type: "int", nullable: false),
                    StartThreshold = table.Column<double>(type: "float", nullable: false),
                    StopThreshold = table.Column<double>(type: "float", nullable: false),
                    MaxDurationSeconds = table.Column<int>(type: "int", nullable: false),
                    PauseBetweenIrrigationsSeconds = table.Column<int>(type: "int", nullable: false),
                    MaxReadingAgeSeconds = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ConfirmedStartThreshold = table.Column<double>(type: "float", nullable: true),
                    ConfirmedStopThreshold = table.Column<double>(type: "float", nullable: true),
                    ConfirmedMaxDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    ConfirmedPauseBetweenIrrigationsSeconds = table.Column<int>(type: "int", nullable: true),
                    ConfirmedMaxReadingAgeSeconds = table.Column<int>(type: "int", nullable: true),
                    ConfirmedIsActive = table.Column<bool>(type: "bit", nullable: true),
                    ConfirmedVersion = table.Column<int>(type: "int", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrrigationConfigs", x => x.IrrigationConfigId);
                    table.CheckConstraint("CK_IrrigationConfig_ThresholdOrder", "[StartThreshold] < [StopThreshold]");
                    table.ForeignKey(
                        name: "FK_IrrigationConfigs_DeviceChannels_MoistureSensorChannelId",
                        column: x => x.MoistureSensorChannelId,
                        principalTable: "DeviceChannels",
                        principalColumn: "DeviceChannelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IrrigationConfigs_IrrigationZones_IrrigationZoneId",
                        column: x => x.IrrigationZoneId,
                        principalTable: "IrrigationZones",
                        principalColumn: "IrrigationZoneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IrrigationZonePlants",
                columns: table => new
                {
                    IrrigationZonePlantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IrrigationZoneId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrrigationZonePlants", x => x.IrrigationZonePlantId);
                    table.ForeignKey(
                        name: "FK_IrrigationZonePlants_IrrigationZones_IrrigationZoneId",
                        column: x => x.IrrigationZoneId,
                        principalTable: "IrrigationZones",
                        principalColumn: "IrrigationZoneId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IrrigationZonePlants_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "PlantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DiagnosisId",
                table: "Schedules",
                column: "DiagnosisId",
                unique: true,
                filter: "[DiagnosisId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_PlotId",
                table: "Plants",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_WorkspaceId",
                table: "Plants",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantPhotos_CameraChannelId",
                table: "PlantPhotos",
                column: "CameraChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SerialNumber",
                table: "Devices",
                column: "SerialNumber",
                unique: true,
                filter: "[SerialNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_WorkspaceId",
                table: "Devices",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_ChannelId_ReadingType_MeasuredAt",
                table: "DeviceReadings",
                columns: new[] { "ChannelId", "ReadingType", "MeasuredAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_ChannelId",
                table: "DeviceEvents",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_DeviceCommandId",
                table: "DeviceEvents",
                column: "DeviceCommandId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_DeviceId",
                table: "DeviceEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_IrrigationZoneId",
                table: "DeviceEvents",
                column: "IrrigationZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_ChannelId",
                table: "DeviceCommands",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_IdempotencyKey",
                table: "DeviceCommands",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_DeviceId",
                table: "Alerts",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_IrrigationZoneId",
                table: "Alerts",
                column: "IrrigationZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_PlantId",
                table: "Alerts",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_WorkspaceId",
                table: "Alerts",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceChannels_DeviceId",
                table: "DeviceChannels",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Farms_WorkspaceId",
                table: "Farms",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationConfigs_IrrigationZoneId",
                table: "IrrigationConfigs",
                column: "IrrigationZoneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationConfigs_MoistureSensorChannelId",
                table: "IrrigationConfigs",
                column: "MoistureSensorChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZonePlants_IrrigationZoneId_PlantId",
                table: "IrrigationZonePlants",
                columns: new[] { "IrrigationZoneId", "PlantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZonePlants_PlantId",
                table: "IrrigationZonePlants",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZones_PlotId",
                table: "IrrigationZones",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZones_ValveChannelId",
                table: "IrrigationZones",
                column: "ValveChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZones_WorkspaceId",
                table: "IrrigationZones",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoCaptureSchedules_CameraChannelId",
                table: "PhotoCaptureSchedules",
                column: "CameraChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoCaptureSchedules_PlantId",
                table: "PhotoCaptureSchedules",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlotObservations_PlotId_SatelliteSceneId",
                table: "PlotObservations",
                columns: new[] { "PlotId", "SatelliteSceneId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlotObservations_SatelliteSceneId",
                table: "PlotObservations",
                column: "SatelliteSceneId");

            migrationBuilder.CreateIndex(
                name: "IX_Plots_FarmId",
                table: "Plots",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_SatelliteChecks_FarmId",
                table: "SatelliteChecks",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_SatelliteScenes_Provider_ExternalId",
                table: "SatelliteScenes",
                columns: new[] { "Provider", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_UserId",
                table: "WorkspaceMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_WorkspaceId_UserId",
                table: "WorkspaceMembers",
                columns: new[] { "WorkspaceId", "UserId" },
                unique: true);

            // Backfill: create one personal Workspace per existing User (as WorkspaceMember/Owner),
            // then point every existing Plant/Device at its owning user's new personal workspace.
            // Verifiable: after this runs, COUNT(Workspaces WHERE Type = 0) = COUNT(Users), every
            // Plant/Device.WorkspaceId is non-zero, and every Plant/Device.WorkspaceId matches the
            // personal workspace of its own UserId (see delivery report for the exact check query).
            migrationBuilder.Sql(@"
                DECLARE @WorkspaceBackfill TABLE (WorkspaceId INT, UserId INT);

                MERGE INTO [Workspaces] AS target
                USING [Users] AS src
                ON 1 = 0
                WHEN NOT MATCHED THEN
                    INSERT ([Type], [Name], [CreatedAt])
                    VALUES (0, CONCAT(src.[Name], N' ', src.[LastName], N' (personal)'), GETUTCDATE())
                OUTPUT inserted.[WorkspaceId], src.[UserId] INTO @WorkspaceBackfill (WorkspaceId, UserId);

                INSERT INTO [WorkspaceMembers] ([WorkspaceId], [UserId], [Role], [JoinedAt])
                SELECT [WorkspaceId], [UserId], 0, GETUTCDATE() FROM @WorkspaceBackfill;

                UPDATE p
                SET p.[WorkspaceId] = wb.[WorkspaceId]
                FROM [Plants] p
                INNER JOIN @WorkspaceBackfill wb ON wb.[UserId] = p.[UserId];

                UPDATE d
                SET d.[WorkspaceId] = wb.[WorkspaceId]
                FROM [Devices] d
                INNER JOIN @WorkspaceBackfill wb ON wb.[UserId] = d.[UserId];
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceCommands_DeviceChannels_ChannelId",
                table: "DeviceCommands",
                column: "ChannelId",
                principalTable: "DeviceChannels",
                principalColumn: "DeviceChannelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceCommands_Devices_DeviceId",
                table: "DeviceCommands",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceEvents_DeviceChannels_ChannelId",
                table: "DeviceEvents",
                column: "ChannelId",
                principalTable: "DeviceChannels",
                principalColumn: "DeviceChannelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceEvents_DeviceCommands_DeviceCommandId",
                table: "DeviceEvents",
                column: "DeviceCommandId",
                principalTable: "DeviceCommands",
                principalColumn: "DeviceCommandId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceEvents_Devices_DeviceId",
                table: "DeviceEvents",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceEvents_IrrigationZones_IrrigationZoneId",
                table: "DeviceEvents",
                column: "IrrigationZoneId",
                principalTable: "IrrigationZones",
                principalColumn: "IrrigationZoneId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceReadings_DeviceChannels_ChannelId",
                table: "DeviceReadings",
                column: "ChannelId",
                principalTable: "DeviceChannels",
                principalColumn: "DeviceChannelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Workspaces_WorkspaceId",
                table: "Devices",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "WorkspaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantPhotos_DeviceChannels_CameraChannelId",
                table: "PlantPhotos",
                column: "CameraChannelId",
                principalTable: "DeviceChannels",
                principalColumn: "DeviceChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_Plots_PlotId",
                table: "Plants",
                column: "PlotId",
                principalTable: "Plots",
                principalColumn: "PlotId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_Workspaces_WorkspaceId",
                table: "Plants",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "WorkspaceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceCommands_DeviceChannels_ChannelId",
                table: "DeviceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceCommands_Devices_DeviceId",
                table: "DeviceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceEvents_DeviceChannels_ChannelId",
                table: "DeviceEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceEvents_DeviceCommands_DeviceCommandId",
                table: "DeviceEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceEvents_Devices_DeviceId",
                table: "DeviceEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceEvents_IrrigationZones_IrrigationZoneId",
                table: "DeviceEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceReadings_DeviceChannels_ChannelId",
                table: "DeviceReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Workspaces_WorkspaceId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantPhotos_DeviceChannels_CameraChannelId",
                table: "PlantPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Plants_Plots_PlotId",
                table: "Plants");

            migrationBuilder.DropForeignKey(
                name: "FK_Plants_Workspaces_WorkspaceId",
                table: "Plants");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "IrrigationConfigs");

            migrationBuilder.DropTable(
                name: "IrrigationZonePlants");

            migrationBuilder.DropTable(
                name: "PhotoCaptureSchedules");

            migrationBuilder.DropTable(
                name: "PlotObservations");

            migrationBuilder.DropTable(
                name: "SatelliteChecks");

            migrationBuilder.DropTable(
                name: "WorkspaceMembers");

            migrationBuilder.DropTable(
                name: "IrrigationZones");

            migrationBuilder.DropTable(
                name: "SatelliteScenes");

            migrationBuilder.DropTable(
                name: "DeviceChannels");

            migrationBuilder.DropTable(
                name: "Plots");

            migrationBuilder.DropTable(
                name: "Farms");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_DiagnosisId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlotId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_WorkspaceId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_PlantPhotos_CameraChannelId",
                table: "PlantPhotos");

            migrationBuilder.DropIndex(
                name: "IX_Devices_SerialNumber",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_WorkspaceId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_ChannelId_ReadingType_MeasuredAt",
                table: "DeviceReadings");

            migrationBuilder.DropIndex(
                name: "IX_DeviceEvents_ChannelId",
                table: "DeviceEvents");

            migrationBuilder.DropIndex(
                name: "IX_DeviceEvents_DeviceCommandId",
                table: "DeviceEvents");

            migrationBuilder.DropIndex(
                name: "IX_DeviceEvents_DeviceId",
                table: "DeviceEvents");

            migrationBuilder.DropIndex(
                name: "IX_DeviceEvents_IrrigationZoneId",
                table: "DeviceEvents");

            migrationBuilder.DropIndex(
                name: "IX_DeviceCommands_ChannelId",
                table: "DeviceCommands");

            migrationBuilder.DropIndex(
                name: "IX_DeviceCommands_IdempotencyKey",
                table: "DeviceCommands");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "ScheduleTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ScheduleTasks");

            migrationBuilder.DropColumn(
                name: "PlotId",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "CameraChannelId",
                table: "PlantPhotos");

            migrationBuilder.DropColumn(
                name: "CapturedAt",
                table: "PlantPhotos");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "PlantPhotos");

            migrationBuilder.DropColumn(
                name: "AiModel",
                table: "Diagnoses");

            migrationBuilder.DropColumn(
                name: "AiModelVersion",
                table: "Diagnoses");

            migrationBuilder.DropColumn(
                name: "AnalysisStatus",
                table: "Diagnoses");

            migrationBuilder.DropColumn(
                name: "HealthStatus",
                table: "Diagnoses");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "MeasuredAt",
                table: "DeviceReadings");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "DeviceEvents");

            migrationBuilder.DropColumn(
                name: "DeviceCommandId",
                table: "DeviceEvents");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "DeviceEvents");

            migrationBuilder.DropColumn(
                name: "IrrigationZoneId",
                table: "DeviceEvents");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "DeviceCommands");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "DeviceCommands");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "DeviceCommands");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "PlantPhotos",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "ReceivedAt",
                table: "DeviceReadings",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "DeviceReadings",
                newName: "PlantDeviceId");

            migrationBuilder.RenameColumn(
                name: "WaterAmountSource",
                table: "DeviceEvents",
                newName: "PlantDeviceId");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "DeviceCommands",
                newName: "PlantDeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceCommands_DeviceId",
                table: "DeviceCommands",
                newName: "IX_DeviceCommands_PlantDeviceId");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "ScheduleTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "DiagnosisId",
                table: "Schedules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHealthy",
                table: "Diagnoses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DiagnosisId",
                table: "Schedules",
                column: "DiagnosisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_PlantDeviceId",
                table: "DeviceReadings",
                column: "PlantDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_PlantDeviceId",
                table: "DeviceEvents",
                column: "PlantDeviceId");

            // NOT re-adding FK_DeviceCommands/DeviceEvents/DeviceReadings_PlantDevices_PlantDeviceId
            // here on purpose (database-reviewer finding, HIGH). By the time anyone rolls back,
            // these columns hold real Device/DeviceChannel ids written after Up() ran — not the
            // original PlantDevice ids Up() overwrote — so re-adding this FK would either fail
            // outright (DeviceReadings: Channel ids essentially never coincide with a valid
            // PlantDeviceId) or silently succeed while linking a command/event to the wrong,
            // unrelated plant/device pairing (Commands/Events: small integer ids can coincidentally
            // collide). The columns are renamed back for schema symmetry, but intentionally left
            // unconstrained — this Down() only reverses the shape, not the data; per
            // references/migrations.md, don't treat this rollback as a real data-recovery path for
            // any database that had writes after Up() applied. A genuine rollback plan for a
            // database with real post-Up() data needs a hand-written, data-aware migration, not
            // this generated one.
        }
    }
}
