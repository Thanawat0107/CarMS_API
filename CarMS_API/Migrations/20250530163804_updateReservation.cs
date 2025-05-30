using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMS_API.Migrations
{
    /// <inheritdoc />
    public partial class updateReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReservationDate",
                table: "Reservations",
                newName: "ReservedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reservations",
                newName: "ExpiryAt");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ReservedAt",
                table: "Reservations",
                newName: "ReservationDate");

            migrationBuilder.RenameColumn(
                name: "ExpiryAt",
                table: "Reservations",
                newName: "CreatedAt");
        }
    }
}
