using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonIdstoTasteProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DislikedGenresIdsJson",
                table: "TasteProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FavoriteGenreIdsJson",
                table: "TasteProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HighlyRatedMovieIdsJson",
                table: "TasteProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DislikedGenresIdsJson",
                table: "TasteProfiles");

            migrationBuilder.DropColumn(
                name: "FavoriteGenreIdsJson",
                table: "TasteProfiles");

            migrationBuilder.DropColumn(
                name: "HighlyRatedMovieIdsJson",
                table: "TasteProfiles");
        }
    }
}
