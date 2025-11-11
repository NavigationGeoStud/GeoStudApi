using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoStud.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsAndLocationSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramId = table.Column<long>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationSuggestions_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    FromTelegramId = table.Column<long>(type: "INTEGER", nullable: true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationSuggestions_LocationId",
                table: "LocationSuggestions",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationSuggestions_Status",
                table: "LocationSuggestions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LocationSuggestions_TelegramId",
                table: "LocationSuggestions",
                column: "TelegramId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationSuggestions_TelegramId_LocationId",
                table: "LocationSuggestions",
                columns: new[] { "TelegramId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_LocationId",
                table: "Notifications",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TelegramId",
                table: "Notifications",
                column: "TelegramId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TelegramId_IsRead",
                table: "Notifications",
                columns: new[] { "TelegramId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationSuggestions");

            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
