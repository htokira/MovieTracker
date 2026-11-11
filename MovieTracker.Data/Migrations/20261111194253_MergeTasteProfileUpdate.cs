using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeTasteProfileUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_TasteProfiles_AspNetUsers_UserId",
                table: "TasteProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TasteProfiles_AspNetUsers_UserId",
                table: "TasteProfiles");
        }
    }
}
