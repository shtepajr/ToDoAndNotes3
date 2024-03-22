using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAndNotes3.Migrations
{
    /// <inheritdoc />
    public partial class removeLabelIsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Labels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Labels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
