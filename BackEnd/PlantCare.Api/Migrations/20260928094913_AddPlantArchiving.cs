using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantCare.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantArchiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Plants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Plants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Plants");
        }
    }
}
