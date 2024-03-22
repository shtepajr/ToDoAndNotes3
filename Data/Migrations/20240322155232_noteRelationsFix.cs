using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAndNotes3.Migrations
{
    /// <inheritdoc />
    public partial class noteRelationsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_NoteDescription_NoteDescriptionId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_NoteDescriptionId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "NoteDescriptionId",
                table: "Notes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoteId",
                table: "NoteDescription",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Labels",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteDescription_NoteId",
                table: "NoteDescription",
                column: "NoteId",
                unique: true,
                filter: "[NoteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_UserId",
                table: "Labels",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Labels_AspNetUsers_UserId",
                table: "Labels",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteDescription_Notes_NoteId",
                table: "NoteDescription",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_UserId",
                table: "Projects",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Labels_AspNetUsers_UserId",
                table: "Labels");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteDescription_Notes_NoteId",
                table: "NoteDescription");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_UserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_UserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_NoteDescription_NoteId",
                table: "NoteDescription");

            migrationBuilder.DropIndex(
                name: "IX_Labels_UserId",
                table: "Labels");

            migrationBuilder.DropColumn(
                name: "NoteId",
                table: "NoteDescription");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoteDescriptionId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Labels",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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
    }
}
