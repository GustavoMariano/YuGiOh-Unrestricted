using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuGiOh_Unrestricted.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MacthcPlayers_Matches_MatchId",
                table: "MacthcPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MacthcPlayers",
                table: "MacthcPlayers");

            migrationBuilder.RenameTable(
                name: "MacthcPlayers",
                newName: "MatchPlayers");

            migrationBuilder.RenameIndex(
                name: "IX_MacthcPlayers_MatchId",
                table: "MatchPlayers",
                newName: "IX_MatchPlayers_MatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchPlayers",
                table: "MatchPlayers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayers_Matches_MatchId",
                table: "MatchPlayers",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayers_Matches_MatchId",
                table: "MatchPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchPlayers",
                table: "MatchPlayers");

            migrationBuilder.RenameTable(
                name: "MatchPlayers",
                newName: "MacthcPlayers");

            migrationBuilder.RenameIndex(
                name: "IX_MatchPlayers_MatchId",
                table: "MacthcPlayers",
                newName: "IX_MacthcPlayers_MatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MacthcPlayers",
                table: "MacthcPlayers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MacthcPlayers_Matches_MatchId",
                table: "MacthcPlayers",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
