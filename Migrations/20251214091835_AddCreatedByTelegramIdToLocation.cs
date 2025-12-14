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
            // Add column for PostgreSQL
            migrationBuilder.AddColumn<long>(
                name: "CreatedByTelegramId",
                table: "Locations",
                type: "bigint",
                nullable: true);

            // Create index for CreatedByTelegramId (as configured in GeoStudDbContext)
            migrationBuilder.CreateIndex(
                name: "IX_Locations_CreatedByTelegramId",
                table: "Locations",
                column: "CreatedByTelegramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_CreatedByTelegramId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedByTelegramId",
                table: "Locations");
        }
    }
}
