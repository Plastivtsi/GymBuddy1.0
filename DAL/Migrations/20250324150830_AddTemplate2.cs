using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Shablon",
                table: "Exercises",
                newName: "Template");

            migrationBuilder.AddColumn<bool>(
                name: "Template",
                table: "Trainings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Template",
                table: "Trainings");

            migrationBuilder.RenameColumn(
                name: "Template",
                table: "Exercises",
                newName: "Shablon");
        }
    }
}
