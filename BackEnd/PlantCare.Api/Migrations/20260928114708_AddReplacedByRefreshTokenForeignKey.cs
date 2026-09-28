using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantCare.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReplacedByRefreshTokenForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ReplacedByRefreshTokenId",
                table: "RefreshTokens",
                column: "ReplacedByRefreshTokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_RefreshTokens_ReplacedByRefreshTokenId",
                table: "RefreshTokens",
                column: "ReplacedByRefreshTokenId",
                principalTable: "RefreshTokens",
                principalColumn: "RefreshTokenId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_RefreshTokens_ReplacedByRefreshTokenId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_ReplacedByRefreshTokenId",
                table: "RefreshTokens");
        }
    }
}
