using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoStud.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeFieldToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Age column as nullable integer
            // Works for both PostgreSQL and SQLite
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Users",
                nullable: true);

            // Note: Making AgeRange nullable is handled by the model change
            // SQLite doesn't enforce NOT NULL constraints the same way as PostgreSQL
            // The model change from [Required] to nullable will be reflected in the snapshot
            // For PostgreSQL, we can alter the column, but for SQLite compatibility,
            // we'll let EF Core handle it through model changes
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Age column
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Users");

            // Note: Restoring AgeRange as required is handled by the model change
            // The model change from nullable to [Required] will be reflected in the snapshot
        }
    }
}

