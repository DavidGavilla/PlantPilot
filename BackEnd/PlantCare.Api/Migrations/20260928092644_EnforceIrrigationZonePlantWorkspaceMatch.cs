using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantCare.Api.Migrations
{
    /// <inheritdoc />
    public partial class EnforceIrrigationZonePlantWorkspaceMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Devices_DeviceId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_IrrigationZonePlants_IrrigationZones_IrrigationZoneId",
                table: "IrrigationZonePlants");

            migrationBuilder.DropForeignKey(
                name: "FK_IrrigationZonePlants_Plants_PlantId",
                table: "IrrigationZonePlants");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantPhotos_DeviceChannels_CameraChannelId",
                table: "PlantPhotos");

            migrationBuilder.DropIndex(
                name: "IX_IrrigationZonePlants_PlantId",
                table: "IrrigationZonePlants");

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "IrrigationZonePlants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill from the Zone side (the more "authoritative" side for an irrigation link),
            // then remove any pre-existing row that turns out to violate the same-workspace
            // invariant this migration introduces — such a row cannot satisfy the composite FK to
            // Plants(PlantId, WorkspaceId) added below, and per "no inventar asociaciones", there
            // is no valid value to invent for it; it represents exactly the cross-workspace link
            // this change exists to prevent, so removing it is correct, not data loss of anything
            // meaningful. Verifiable: 0 rows should ever match the DELETE's WHERE clause once this
            // migration is applied to a database that was already enforcing the invariant at the
            // application layer; if it deletes something on a database with real usage, that is
            // itself a signal a bug let a bad link through before this migration existed.
            migrationBuilder.Sql(@"
                UPDATE izp
                SET izp.WorkspaceId = iz.WorkspaceId
                FROM [IrrigationZonePlants] izp
                INNER JOIN [IrrigationZones] iz ON iz.[IrrigationZoneId] = izp.[IrrigationZoneId];

                DELETE izp
                FROM [IrrigationZonePlants] izp
                INNER JOIN [Plants] p ON p.[PlantId] = izp.[PlantId]
                WHERE p.[WorkspaceId] <> izp.[WorkspaceId];
            ");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Plants_PlantId_WorkspaceId",
                table: "Plants",
                columns: new[] { "PlantId", "WorkspaceId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_IrrigationZones_IrrigationZoneId_WorkspaceId",
                table: "IrrigationZones",
                columns: new[] { "IrrigationZoneId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZonePlants_IrrigationZoneId_WorkspaceId",
                table: "IrrigationZonePlants",
                columns: new[] { "IrrigationZoneId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZonePlants_PlantId_WorkspaceId",
                table: "IrrigationZonePlants",
                columns: new[] { "PlantId", "WorkspaceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Devices_DeviceId",
                table: "Alerts",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_IrrigationZonePlants_IrrigationZones_IrrigationZoneId_WorkspaceId",
                table: "IrrigationZonePlants",
                columns: new[] { "IrrigationZoneId", "WorkspaceId" },
                principalTable: "IrrigationZones",
                principalColumns: new[] { "IrrigationZoneId", "WorkspaceId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IrrigationZonePlants_Plants_PlantId_WorkspaceId",
                table: "IrrigationZonePlants",
                columns: new[] { "PlantId", "WorkspaceId" },
                principalTable: "Plants",
                principalColumns: new[] { "PlantId", "WorkspaceId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantPhotos_DeviceChannels_CameraChannelId",
                table: "PlantPhotos",
                column: "CameraChannelId",
                principalTable: "DeviceChannels",
                principalColumn: "DeviceChannelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Devices_DeviceId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_IrrigationZonePlants_IrrigationZones_IrrigationZoneId_WorkspaceId",
                table: "IrrigationZonePlants");

            migrationBuilder.DropForeignKey(
                name: "FK_IrrigationZonePlants_Plants_PlantId_WorkspaceId",
                table: "IrrigationZonePlants");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantPhotos_DeviceChannels_CameraChannelId",
                table: "PlantPhotos");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Plants_PlantId_WorkspaceId",
                table: "Plants");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_IrrigationZones_IrrigationZoneId_WorkspaceId",
                table: "IrrigationZones");

            migrationBuilder.DropIndex(
                name: "IX_IrrigationZonePlants_IrrigationZoneId_WorkspaceId",
                table: "IrrigationZonePlants");

            migrationBuilder.DropIndex(
                name: "IX_IrrigationZonePlants_PlantId_WorkspaceId",
                table: "IrrigationZonePlants");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "IrrigationZonePlants");

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationZonePlants_PlantId",
                table: "IrrigationZonePlants",
                column: "PlantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Devices_DeviceId",
                table: "Alerts",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IrrigationZonePlants_IrrigationZones_IrrigationZoneId",
                table: "IrrigationZonePlants",
                column: "IrrigationZoneId",
                principalTable: "IrrigationZones",
                principalColumn: "IrrigationZoneId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IrrigationZonePlants_Plants_PlantId",
                table: "IrrigationZonePlants",
                column: "PlantId",
                principalTable: "Plants",
                principalColumn: "PlantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantPhotos_DeviceChannels_CameraChannelId",
                table: "PlantPhotos",
                column: "CameraChannelId",
                principalTable: "DeviceChannels",
                principalColumn: "DeviceChannelId");
        }
    }
}
