using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Approvals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CarHistoryId = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Approvals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Approvals_CarHistories_CarHistoryId",
                        column: x => x.CarHistoryId,
                        principalTable: "CarHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_CarHistoryId",
                table: "Approvals",
                column: "CarHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_UserId",
                table: "Approvals",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Approvals");
        }
    }
}
