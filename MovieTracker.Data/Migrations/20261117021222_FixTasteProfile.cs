using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTasteProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DislikedGenresJson",
                table: "TasteProfiles");

            migrationBuilder.DropColumn(
                name: "FavoriteGenresJson",
                table: "TasteProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DislikedGenresJson",
                table: "TasteProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FavoriteGenresJson",
                table: "TasteProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
