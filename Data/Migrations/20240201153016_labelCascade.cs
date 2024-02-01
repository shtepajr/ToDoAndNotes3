using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAndNotes3.Migrations
{
    /// <inheritdoc />
    public partial class labelCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteLabel_Labels_LabelId",
                table: "NoteLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLabel_Labels_LabelId",
                table: "TaskLabel");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteLabel_Labels_LabelId",
                table: "NoteLabel",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "LabelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLabel_Labels_LabelId",
                table: "TaskLabel",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "LabelId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteLabel_Labels_LabelId",
                table: "NoteLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLabel_Labels_LabelId",
                table: "TaskLabel");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteLabel_Labels_LabelId",
                table: "NoteLabel",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "LabelId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLabel_Labels_LabelId",
                table: "TaskLabel",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "LabelId");
        }
    }
}
