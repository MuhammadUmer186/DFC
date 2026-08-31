using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StorageKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Sha256Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SyncState = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
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
                    table.PrimaryKey("PK_UploadedFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_BranchId_UpdatedAtUtc",
                table: "UploadedFiles",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_DeletedAtUtc",
                table: "UploadedFiles",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_GlobalId",
                table: "UploadedFiles",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_Sha256Hash",
                table: "UploadedFiles",
                column: "Sha256Hash");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_StorageKey",
                table: "UploadedFiles",
                column: "StorageKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedFiles");
        }
    }
}
