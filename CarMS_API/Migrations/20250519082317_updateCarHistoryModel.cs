using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMS_API.Migrations
{
    /// <inheritdoc />
    public partial class updateCarHistoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Source",
                table: "CarHistories",
                newName: "Insurance");

            migrationBuilder.AddColumn<string>(
                name: "Act",
                table: "CarHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Finance",
                table: "CarHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCollisionHistory",
                table: "CarHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CarHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "CarHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Act",
                table: "CarHistories");

            migrationBuilder.DropColumn(
                name: "Finance",
                table: "CarHistories");

            migrationBuilder.DropColumn(
                name: "IsCollisionHistory",
                table: "CarHistories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CarHistories");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "CarHistories");

            migrationBuilder.RenameColumn(
                name: "Insurance",
                table: "CarHistories",
                newName: "Source");
        }
    }
}
