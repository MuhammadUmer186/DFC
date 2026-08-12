using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderSerialSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OrderSerialCurrentDate",
                table: "SiteSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderSerialCurrentNumber",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OrderSerialPrefix",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OrderSerialResetTime",
                table: "SiteSettings",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "OrderSerialStartingNumber",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "OrderSerialCurrentDate", "OrderSerialCurrentNumber", "OrderSerialPrefix", "OrderSerialResetTime", "OrderSerialStartingNumber" },
                values: new object[] { null, 0, "", new TimeSpan(0, 0, 0, 0, 0), 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderSerialCurrentDate",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "OrderSerialCurrentNumber",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "OrderSerialPrefix",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "OrderSerialResetTime",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "OrderSerialStartingNumber",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Orders");
        }
    }
}
