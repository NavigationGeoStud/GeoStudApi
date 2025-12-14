using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoStud.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByTelegramIdToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedByTelegramId",
                table: "Locations",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByTelegramId",
                table: "Locations");
        }
    }
}
