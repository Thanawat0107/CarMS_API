using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMS_API.Migrations
{
    /// <inheritdoc />
    public partial class updateCarHistoryModel3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "CarHistories");

            migrationBuilder.RenameColumn(
                name: "VerifiedDate",
                table: "CarHistories",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "CarHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "CarHistories");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "CarHistories",
                newName: "VerifiedDate");

            migrationBuilder.AddColumn<string>(
                name: "VerifiedBy",
                table: "CarHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
