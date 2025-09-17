using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuGiOh_Unrestricted.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCardIdToCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardId",
                table: "Cards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardId",
                table: "Cards");
        }
    }
}
