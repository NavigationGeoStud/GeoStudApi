using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoStud.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNeedModerateToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add NeedModerate column - EF Core will handle type conversion
            // PostgreSQL uses 'boolean', SQLite uses 'INTEGER'
            migrationBuilder.AddColumn<bool>(
                name: "NeedModerate",
                table: "Locations",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedModerate",
                table: "Locations");
        }
    }
}
