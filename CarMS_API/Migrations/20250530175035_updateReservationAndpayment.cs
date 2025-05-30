using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMS_API.Migrations
{
    /// <inheritdoc />
    public partial class updateReservationAndpayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Payments",
                newName: "TransactionRef");

            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Payments",
                newName: "PaidAt");

            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Method",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "TransactionRef",
                table: "Payments",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "PaidAt",
                table: "Payments",
                newName: "PaymentDate");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
