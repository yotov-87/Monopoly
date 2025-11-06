using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monopoly.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelsToBoardCell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Hotels",
                table: "BoardCells",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hotels",
                table: "BoardCells");
        }
    }
}
