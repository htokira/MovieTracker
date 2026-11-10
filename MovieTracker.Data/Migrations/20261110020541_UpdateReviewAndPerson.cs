using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewAndPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MoviePersons",
                table: "MoviePersons");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Persons",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Persons",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MoviePersons",
                table: "MoviePersons",
                columns: new[] { "MovieId", "PersonId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MoviePersons",
                table: "MoviePersons");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Persons");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Persons",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MoviePersons",
                table: "MoviePersons",
                columns: new[] { "MovieId", "PersonId", "Role" });
        }
    }
}
