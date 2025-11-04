using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Monopoly.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyTradeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyTrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    CellPosition = table.Column<int>(type: "integer", nullable: false),
                    BuyerPlayerId = table.Column<int>(type: "integer", nullable: false),
                    SellerPlayerId = table.Column<int>(type: "integer", nullable: false),
                    OfferedPrice = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyTrades_GamePlayers_BuyerPlayerId",
                        column: x => x.BuyerPlayerId,
                        principalTable: "GamePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyTrades_GamePlayers_SellerPlayerId",
                        column: x => x.SellerPlayerId,
                        principalTable: "GamePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyTrades_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyTrades_BuyerPlayerId",
                table: "PropertyTrades",
                column: "BuyerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyTrades_GameId",
                table: "PropertyTrades",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyTrades_SellerPlayerId",
                table: "PropertyTrades",
                column: "SellerPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyTrades");
        }
    }
}
