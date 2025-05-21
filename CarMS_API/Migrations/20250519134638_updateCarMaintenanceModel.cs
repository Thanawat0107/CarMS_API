using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMS_API.Migrations
{
    /// <inheritdoc />
    public partial class updateCarMaintenanceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarMaintenances_Cars_CarId",
                table: "CarMaintenances");

            migrationBuilder.DropIndex(
                name: "IX_CarMaintenances_CarId",
                table: "CarMaintenances");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "CarMaintenances");

            migrationBuilder.AddColumn<int>(
                name: "CarHistoryId",
                table: "CarMaintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CarMaintenances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "CarMaintenances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CarMaintenances_CarHistoryId",
                table: "CarMaintenances",
                column: "CarHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarMaintenances_CarHistories_CarHistoryId",
                table: "CarMaintenances",
                column: "CarHistoryId",
                principalTable: "CarHistories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarMaintenances_CarHistories_CarHistoryId",
                table: "CarMaintenances");

            migrationBuilder.DropIndex(
                name: "IX_CarMaintenances_CarHistoryId",
                table: "CarMaintenances");

            migrationBuilder.DropColumn(
                name: "CarHistoryId",
                table: "CarMaintenances");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CarMaintenances");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "CarMaintenances");

            migrationBuilder.AddColumn<int>(
                name: "CarId",
                table: "CarMaintenances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CarMaintenances_CarId",
                table: "CarMaintenances",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarMaintenances_Cars_CarId",
                table: "CarMaintenances",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
