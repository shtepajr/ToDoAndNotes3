using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAndNotes3.Migrations
{
    /// <inheritdoc />
    public partial class addNoteDescriptionSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "NoteDescriptionId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NoteDescription",
                columns: table => new
                {
                    NoteDescriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteDescription", x => x.NoteDescriptionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_NoteDescriptionId",
                table: "Notes",
                column: "NoteDescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_NoteDescription_NoteDescriptionId",
                table: "Notes",
                column: "NoteDescriptionId",
                principalTable: "NoteDescription",
                principalColumn: "NoteDescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_NoteDescription_NoteDescriptionId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "NoteDescription");

            migrationBuilder.DropIndex(
                name: "IX_Notes_NoteDescriptionId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "NoteDescriptionId",
                table: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
