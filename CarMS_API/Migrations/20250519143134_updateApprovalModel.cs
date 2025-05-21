using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMS_API.Migrations
{
    /// <inheritdoc />
    public partial class updateApprovalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Approvals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Approvals");
        }
    }
}
