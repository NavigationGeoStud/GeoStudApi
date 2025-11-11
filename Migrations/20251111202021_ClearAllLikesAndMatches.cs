using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoStud.Api.Migrations
{
    /// <inheritdoc />
    public partial class ClearAllLikesAndMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Удаляем все уведомления типа "like" и "match"
            migrationBuilder.Sql("DELETE FROM Notifications WHERE Type IN ('like', 'match');");
            
            // Удаляем все мэтчи
            migrationBuilder.Sql("DELETE FROM Matches;");
            
            // Удаляем все дизлайки
            migrationBuilder.Sql("DELETE FROM UserDislikes;");
            
            // Удаляем все лайки
            migrationBuilder.Sql("DELETE FROM UserLikes;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Откат невозможен, так как данные удаляются физически
            // Эта миграция необратима
        }
    }
}
