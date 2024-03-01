using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAndNotes3.Migrations
{
    /// <inheritdoc />
    public partial class changeUserNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUsers",
                newName: "CustomName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomName",
                table: "AspNetUsers",
                newName: "Name");
        }
    }
}
