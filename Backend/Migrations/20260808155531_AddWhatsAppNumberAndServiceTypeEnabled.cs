using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppNumberAndServiceTypeEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "ServiceTimeSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ServiceTimeSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "ServiceTimeSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "ServiceTimeSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "WhatsAppNumber",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "ServiceTimeSettings");
        }
    }
}
