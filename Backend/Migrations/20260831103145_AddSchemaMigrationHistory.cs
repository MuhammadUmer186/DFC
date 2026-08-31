using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaMigrationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchemaMigrationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FromMigration = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToMigration = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AppliedCount = table.Column<int>(type: "int", nullable: false),
                    AppVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NodeRole = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BackupPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    BackupTaken = table.Column<bool>(type: "bit", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaMigrationHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchemaMigrationHistories_StartedAtUtc",
                table: "SchemaMigrationHistories",
                column: "StartedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchemaMigrationHistories");
        }
    }
}
