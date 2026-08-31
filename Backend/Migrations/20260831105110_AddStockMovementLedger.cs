using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RawItemId = table.Column<int>(type: "int", nullable: false),
                    RawItemGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    VendorGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuantityDelta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ReferenceGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReversesMovementGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "'00000000-0000-0000-0000-000000000000'"),
                    OriginNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "'00000000-0000-0000-0000-000000000000'"),
                    AggregateVersion = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BranchId_UpdatedAtUtc",
                table: "StockMovements",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_DeletedAtUtc",
                table: "StockMovements",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_GlobalId",
                table: "StockMovements",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_OccurredAtUtc",
                table: "StockMovements",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_RawItemId_VendorId",
                table: "StockMovements",
                columns: new[] { "RawItemId", "VendorId" });

            migrationBuilder.CreateIndex(
                name: "UX_StockMovements_Reference",
                table: "StockMovements",
                columns: new[] { "ReferenceType", "ReferenceGlobalId", "MovementType", "RawItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");
        }
    }
}
