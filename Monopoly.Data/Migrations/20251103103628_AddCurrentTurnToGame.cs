using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monopoly.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentTurnToGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentTurnUserId",
                table: "Games",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_CurrentTurnUserId",
                table: "Games",
                column: "CurrentTurnUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Users_CurrentTurnUserId",
                table: "Games",
                column: "CurrentTurnUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Users_CurrentTurnUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_CurrentTurnUserId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CurrentTurnUserId",
                table: "Games");
        }
    }
}
