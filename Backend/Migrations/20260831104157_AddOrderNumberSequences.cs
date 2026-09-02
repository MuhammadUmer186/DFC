using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumberSource",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderNumberSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    BusinessDate = table.Column<DateTime>(type: "date", nullable: false),
                    LastValue = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNumberSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumberSource",
                table: "Orders",
                column: "OrderNumberSource");

            migrationBuilder.CreateIndex(
                name: "UX_Orders_OrderNumber_Sequenced",
                table: "Orders",
                column: "OrderNumber",
                unique: true,
                filter: "[OrderNumberSource] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNumberSequences_BranchId_SourceCode_BusinessDate",
                table: "OrderNumberSequences",
                columns: new[] { "BranchId", "SourceCode", "BusinessDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderNumberSequences");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumberSource",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "UX_Orders_OrderNumber_Sequenced",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderNumberSource",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
