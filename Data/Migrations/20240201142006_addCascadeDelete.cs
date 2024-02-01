using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAndNotes3.Migrations
{
    /// <inheritdoc />
    public partial class addCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteLabel_Notes_TaskNoteId",
                table: "NoteLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Projects_ProjectId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLabel_Tasks_TaskId",
                table: "TaskLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "TaskNoteId",
                table: "NoteLabel",
                newName: "NoteId");

            migrationBuilder.RenameIndex(
                name: "IX_NoteLabel_TaskNoteId",
                table: "NoteLabel",
                newName: "IX_NoteLabel_NoteId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteLabel_Notes_NoteId",
                table: "NoteLabel",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Projects_ProjectId",
                table: "Notes",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLabel_Tasks_TaskId",
                table: "TaskLabel",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteLabel_Notes_NoteId",
                table: "NoteLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Projects_ProjectId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLabel_Tasks_TaskId",
                table: "TaskLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "NoteId",
                table: "NoteLabel",
                newName: "TaskNoteId");

            migrationBuilder.RenameIndex(
                name: "IX_NoteLabel_NoteId",
                table: "NoteLabel",
                newName: "IX_NoteLabel_TaskNoteId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteLabel_Notes_TaskNoteId",
                table: "NoteLabel",
                column: "TaskNoteId",
                principalTable: "Notes",
                principalColumn: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Projects_ProjectId",
                table: "Notes",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLabel_Tasks_TaskId",
                table: "TaskLabel",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId");
        }
    }
}
