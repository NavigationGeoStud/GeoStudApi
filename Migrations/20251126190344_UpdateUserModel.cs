using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoStud.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Age column - works for both SQLite and PostgreSQL
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Users",
                nullable: true);

            // Make AgeRange nullable - works for both SQLite and PostgreSQL
            // EF Core will handle the type conversion based on the provider
            migrationBuilder.AlterColumn<string>(
                name: "AgeRange",
                table: "Users",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Age column
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Users");

            // Restore AgeRange as required (non-nullable)
            // Note: For existing data, you may need to set a default value before making it non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "AgeRange",
                table: "Users",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
