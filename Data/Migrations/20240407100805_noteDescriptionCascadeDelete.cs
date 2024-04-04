using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAndNotes3.Migrations
{
    /// <inheritdoc />
    public partial class noteDescriptionCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteDescription_Notes_NoteId",
                table: "NoteDescription");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteDescription_Notes_NoteId",
                table: "NoteDescription",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteDescription_Notes_NoteId",
                table: "NoteDescription");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteDescription_Notes_NoteId",
                table: "NoteDescription",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId");
        }
    }
}
