using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncIdentityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "WasteRecords",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "WasteRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "WasteRecords",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "WasteRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "WasteRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "WasteRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WasteRecords",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "WasteRecords",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "WasteItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "WasteItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "WasteItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Vendors",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Vendors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Vendors",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Vendors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Vendors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Vendors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Vendors",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Vendors",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Users",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "SiteSettings",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "SiteSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "SiteSettings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "SiteSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "SiteSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "SiteSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SiteSettings",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "SiteSettings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "ServiceTimeSettings",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "ServiceTimeSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ServiceTimeSettings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "ServiceTimeSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "ServiceTimeSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "ServiceTimeSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceTimeSettings",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ServiceTimeSettings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Riders",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Riders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Riders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Riders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Riders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Riders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Riders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Riders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "RawItems",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "RawItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "RawItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "RawItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "RawItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "RawItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RawItems",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "RawItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "PurchaseOrders",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PurchaseOrders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "PurchaseOrderItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "PurchaseOrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "PurchaseOrderItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Orders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "OrderItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "OrderItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "OrderDeals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "OrderDeals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "OrderDeals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "MenuRecipes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "MenuRecipes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "MenuRecipes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "MenuItems",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "MenuItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MenuItems",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "MenuItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "KitchenOuts",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "KitchenOuts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "KitchenOuts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "KitchenOuts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "KitchenOuts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "KitchenOuts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "KitchenOuts",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "KitchenOuts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "KitchenOutItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "KitchenOutItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "KitchenOutItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Deals",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Deals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Deals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Deals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Deals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Deals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Deals",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Deals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "DealItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "DealItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "DealItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Customers",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Customers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Categories",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Categories",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "Areas",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Areas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Areas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Areas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Areas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginNodeId",
                table: "Areas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "'00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Areas",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Areas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.CreateTable(
                name: "SyncTombstones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AggregateVersion = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dispatched = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncTombstones", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAtUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTimeSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAtUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTimeSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "DeletedAtUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTimeSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "DeletedAtUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAtUtc",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_WasteRecords_BranchId_UpdatedAtUtc",
                table: "WasteRecords",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WasteRecords_DeletedAtUtc",
                table: "WasteRecords",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WasteRecords_GlobalId",
                table: "WasteRecords",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WasteItems_GlobalId",
                table: "WasteItems",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_BranchId_UpdatedAtUtc",
                table: "Vendors",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_DeletedAtUtc",
                table: "Vendors",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_GlobalId",
                table: "Vendors",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId_UpdatedAtUtc",
                table: "Users",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedAtUtc",
                table: "Users",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GlobalId",
                table: "Users",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_BranchId_UpdatedAtUtc",
                table: "SiteSettings",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_DeletedAtUtc",
                table: "SiteSettings",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_GlobalId",
                table: "SiteSettings",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTimeSettings_BranchId_UpdatedAtUtc",
                table: "ServiceTimeSettings",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTimeSettings_DeletedAtUtc",
                table: "ServiceTimeSettings",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTimeSettings_GlobalId",
                table: "ServiceTimeSettings",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Riders_BranchId_UpdatedAtUtc",
                table: "Riders",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Riders_DeletedAtUtc",
                table: "Riders",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Riders_GlobalId",
                table: "Riders",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawItems_BranchId_UpdatedAtUtc",
                table: "RawItems",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RawItems_DeletedAtUtc",
                table: "RawItems",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RawItems_GlobalId",
                table: "RawItems",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BranchId_UpdatedAtUtc",
                table: "PurchaseOrders",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_DeletedAtUtc",
                table: "PurchaseOrders",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_GlobalId",
                table: "PurchaseOrders",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_GlobalId",
                table: "PurchaseOrderItems",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BranchId_UpdatedAtUtc",
                table: "Orders",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeletedAtUtc",
                table: "Orders",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GlobalId",
                table: "Orders",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_GlobalId",
                table: "OrderItems",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDeals_GlobalId",
                table: "OrderDeals",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuRecipes_GlobalId",
                table: "MenuRecipes",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_BranchId_UpdatedAtUtc",
                table: "MenuItems",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_DeletedAtUtc",
                table: "MenuItems",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_GlobalId",
                table: "MenuItems",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenOuts_BranchId_UpdatedAtUtc",
                table: "KitchenOuts",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_KitchenOuts_DeletedAtUtc",
                table: "KitchenOuts",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_KitchenOuts_GlobalId",
                table: "KitchenOuts",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenOutItems_GlobalId",
                table: "KitchenOutItems",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deals_BranchId_UpdatedAtUtc",
                table: "Deals",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Deals_DeletedAtUtc",
                table: "Deals",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_GlobalId",
                table: "Deals",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DealItems_GlobalId",
                table: "DealItems",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BranchId_UpdatedAtUtc",
                table: "Customers",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DeletedAtUtc",
                table: "Customers",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GlobalId",
                table: "Customers",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BranchId_UpdatedAtUtc",
                table: "Categories",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DeletedAtUtc",
                table: "Categories",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_GlobalId",
                table: "Categories",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Areas_BranchId_UpdatedAtUtc",
                table: "Areas",
                columns: new[] { "BranchId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Areas_DeletedAtUtc",
                table: "Areas",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_GlobalId",
                table: "Areas",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncTombstones_Dispatched_DeletedAtUtc",
                table: "SyncTombstones",
                columns: new[] { "Dispatched", "DeletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncTombstones_GlobalId",
                table: "SyncTombstones",
                column: "GlobalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncTombstones");

            migrationBuilder.DropIndex(
                name: "IX_WasteRecords_BranchId_UpdatedAtUtc",
                table: "WasteRecords");

            migrationBuilder.DropIndex(
                name: "IX_WasteRecords_DeletedAtUtc",
                table: "WasteRecords");

            migrationBuilder.DropIndex(
                name: "IX_WasteRecords_GlobalId",
                table: "WasteRecords");

            migrationBuilder.DropIndex(
                name: "IX_WasteItems_GlobalId",
                table: "WasteItems");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_BranchId_UpdatedAtUtc",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_DeletedAtUtc",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_GlobalId",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Users_BranchId_UpdatedAtUtc",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DeletedAtUtc",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_GlobalId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SiteSettings_BranchId_UpdatedAtUtc",
                table: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_SiteSettings_DeletedAtUtc",
                table: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_SiteSettings_GlobalId",
                table: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTimeSettings_BranchId_UpdatedAtUtc",
                table: "ServiceTimeSettings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTimeSettings_DeletedAtUtc",
                table: "ServiceTimeSettings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTimeSettings_GlobalId",
                table: "ServiceTimeSettings");

            migrationBuilder.DropIndex(
                name: "IX_Riders_BranchId_UpdatedAtUtc",
                table: "Riders");

            migrationBuilder.DropIndex(
                name: "IX_Riders_DeletedAtUtc",
                table: "Riders");

            migrationBuilder.DropIndex(
                name: "IX_Riders_GlobalId",
                table: "Riders");

            migrationBuilder.DropIndex(
                name: "IX_RawItems_BranchId_UpdatedAtUtc",
                table: "RawItems");

            migrationBuilder.DropIndex(
                name: "IX_RawItems_DeletedAtUtc",
                table: "RawItems");

            migrationBuilder.DropIndex(
                name: "IX_RawItems_GlobalId",
                table: "RawItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BranchId_UpdatedAtUtc",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_DeletedAtUtc",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_GlobalId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_GlobalId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BranchId_UpdatedAtUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeletedAtUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_GlobalId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_GlobalId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderDeals_GlobalId",
                table: "OrderDeals");

            migrationBuilder.DropIndex(
                name: "IX_MenuRecipes_GlobalId",
                table: "MenuRecipes");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_BranchId_UpdatedAtUtc",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_DeletedAtUtc",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_GlobalId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_KitchenOuts_BranchId_UpdatedAtUtc",
                table: "KitchenOuts");

            migrationBuilder.DropIndex(
                name: "IX_KitchenOuts_DeletedAtUtc",
                table: "KitchenOuts");

            migrationBuilder.DropIndex(
                name: "IX_KitchenOuts_GlobalId",
                table: "KitchenOuts");

            migrationBuilder.DropIndex(
                name: "IX_KitchenOutItems_GlobalId",
                table: "KitchenOutItems");

            migrationBuilder.DropIndex(
                name: "IX_Deals_BranchId_UpdatedAtUtc",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_DeletedAtUtc",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_GlobalId",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_DealItems_GlobalId",
                table: "DealItems");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BranchId_UpdatedAtUtc",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DeletedAtUtc",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_GlobalId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Categories_BranchId_UpdatedAtUtc",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_DeletedAtUtc",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_GlobalId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Areas_BranchId_UpdatedAtUtc",
                table: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Areas_DeletedAtUtc",
                table: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Areas_GlobalId",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "WasteItems");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "WasteItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "WasteItems");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ServiceTimeSettings");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "OrderDeals");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "OrderDeals");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "OrderDeals");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "MenuRecipes");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "MenuRecipes");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "MenuRecipes");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "KitchenOuts");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "KitchenOutItems");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "KitchenOutItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "KitchenOutItems");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "DealItems");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "DealItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "DealItems");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "OriginNodeId",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Areas");
        }
    }
}
