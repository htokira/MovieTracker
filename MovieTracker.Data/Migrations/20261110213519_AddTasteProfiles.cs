using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTasteProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TasteProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReviewsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    GenrePreferencesJson = table.Column<string>(type: "TEXT", nullable: false),
                    FavoriteGenresJson = table.Column<string>(type: "TEXT", nullable: false),
                    DislikedGenresJson = table.Column<string>(type: "TEXT", nullable: false),
                    TasteDiversity = table.Column<double>(type: "REAL", nullable: false),
                    TasteConsistency = table.Column<double>(type: "REAL", nullable: false),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TasteProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TasteProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TasteProfiles_UserId",
                table: "TasteProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TasteProfiles");
        }
    }
}
