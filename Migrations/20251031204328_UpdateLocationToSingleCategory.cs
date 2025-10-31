using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoStud.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLocationToSingleCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add CategoryId as nullable first
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Locations",
                type: "INTEGER",
                nullable: true);

            // Step 2: Update CategoryId from LocationCategoryJoins
            // If a location has multiple categories, take the first one
            migrationBuilder.Sql(@"
                UPDATE Locations
                SET CategoryId = (
                    SELECT CategoryId 
                    FROM LocationCategoryJoins 
                    WHERE LocationCategoryJoins.LocationId = Locations.Id 
                    LIMIT 1
                )
                WHERE EXISTS (
                    SELECT 1 
                    FROM LocationCategoryJoins 
                    WHERE LocationCategoryJoins.LocationId = Locations.Id
                );
            ");

            // Step 3: For locations without category, assign a default category (first available)
            migrationBuilder.Sql(@"
                UPDATE Locations
                SET CategoryId = (SELECT Id FROM LocationCategories LIMIT 1)
                WHERE CategoryId IS NULL;
            ");

            // Step 4: Drop LocationCategoryJoins table before recreating Locations
            migrationBuilder.DropTable(
                name: "LocationCategoryJoins");
            
            // Step 5: Make CategoryId NOT NULL by recreating table
            // SQLite limitation: need to recreate table to change NULL to NOT NULL
            migrationBuilder.Sql(@"
                PRAGMA foreign_keys = 0;
                
                CREATE TABLE Locations_new (
                    Id INTEGER NOT NULL CONSTRAINT PK_Locations PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    Coordinates TEXT NOT NULL,
                    Address TEXT NULL,
                    City TEXT NULL,
                    Phone TEXT NULL,
                    Website TEXT NULL,
                    ImageUrl TEXT NULL,
                    Rating TEXT NULL,
                    RatingCount INTEGER NULL,
                    PriceRange TEXT NULL,
                    WorkingHours TEXT NULL,
                    IsActive INTEGER NOT NULL,
                    IsVerified INTEGER NOT NULL,
                    CategoryId INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (GETUTCDATE()),
                    UpdatedAt TEXT NULL,
                    IsDeleted INTEGER NOT NULL
                );
                
                INSERT INTO Locations_new 
                (Id, Name, Description, Coordinates, Address, City, Phone, Website, ImageUrl, 
                 Rating, RatingCount, PriceRange, WorkingHours, IsActive, IsVerified, CategoryId, 
                 CreatedAt, UpdatedAt, IsDeleted)
                SELECT Id, Name, Description, Coordinates, Address, City, Phone, Website, ImageUrl,
                       Rating, RatingCount, PriceRange, WorkingHours, IsActive, IsVerified, CategoryId,
                       CreatedAt, UpdatedAt, IsDeleted
                FROM Locations;
                
                DROP TABLE Locations;
                ALTER TABLE Locations_new RENAME TO Locations;
                
                PRAGMA foreign_keys = 1;
            ");
            
            // Step 6: Create index after table recreation
            migrationBuilder.CreateIndex(
                name: "IX_Locations_CategoryId",
                table: "Locations",
                column: "CategoryId");

            // Step 7: Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Locations_LocationCategories_CategoryId",
                table: "Locations",
                column: "CategoryId",
                principalTable: "LocationCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_LocationCategories_CategoryId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_CategoryId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Locations");

            migrationBuilder.CreateTable(
                name: "LocationCategoryJoins",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationCategoryJoins", x => new { x.LocationId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_LocationCategoryJoins_LocationCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "LocationCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationCategoryJoins_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationCategoryJoins_CategoryId",
                table: "LocationCategoryJoins",
                column: "CategoryId");
        }
    }
}
