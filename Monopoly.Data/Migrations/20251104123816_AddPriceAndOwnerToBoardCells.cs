using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monopoly.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceAndOwnerToBoardCells : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "BoardCells",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "BoardCells",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardCells_OwnerId",
                table: "BoardCells",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardCells_GamePlayers_OwnerId",
                table: "BoardCells",
                column: "OwnerId",
                principalTable: "GamePlayers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardCells_GamePlayers_OwnerId",
                table: "BoardCells");

            migrationBuilder.DropIndex(
                name: "IX_BoardCells_OwnerId",
                table: "BoardCells");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "BoardCells");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "BoardCells");
        }
    }
}
