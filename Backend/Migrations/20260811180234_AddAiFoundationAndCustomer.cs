using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAiFoundationAndCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                table: "Vendors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumOrderQuantity",
                table: "Vendors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SafetyStockQuantity",
                table: "RawItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                table: "RawItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AiAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Feature = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InputTokens = table.Column<int>(type: "int", nullable: true),
                    OutputTokens = table.Column<int>(type: "int", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiForecastRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForecastFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ForecastTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mae = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Wape = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiForecastRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiInventoryRecommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RawItemId = table.Column<int>(type: "int", nullable: false),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ForecastedDemand = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SuggestedReorderQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SuggestedReorderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RecommendationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataWarnings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceLow = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConfidenceHigh = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiInventoryRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiInventoryRecommendations_RawItems_RawItemId",
                        column: x => x.RawItemId,
                        principalTable: "RawItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiToolExecutionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArgumentsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ResultSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiToolExecutionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalizationConsent = table.Column<bool>(type: "bit", nullable: false),
                    Allergens = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DietaryPreferences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiMessageRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiMessageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiMessageRecords_AiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiForecastValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForecastRunId = table.Column<int>(type: "int", nullable: false),
                    ForecastDate = table.Column<DateOnly>(type: "date", nullable: false),
                    HourOfDay = table.Column<int>(type: "int", nullable: true),
                    MenuItemId = table.Column<int>(type: "int", nullable: true),
                    PredictedSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PredictedOrderCount = table.Column<int>(type: "int", nullable: false),
                    PredictedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConfidenceLow = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConfidenceHigh = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LowConfidence = table.Column<bool>(type: "bit", nullable: false),
                    ActualSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ActualOrderCount = table.Column<int>(type: "int", nullable: true),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiForecastValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiForecastValues_AiForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "AiForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiForecastValues_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AiInventoryRecommendationDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecommendationId = table.Column<int>(type: "int", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedByUserId = table.Column<int>(type: "int", nullable: true),
                    DecidedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiInventoryRecommendationDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiInventoryRecommendationDecisions_AiInventoryRecommendations_RecommendationId",
                        column: x => x.RecommendationId,
                        principalTable: "AiInventoryRecommendations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAuditLogs_CreatedAt",
                table: "AiAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiAuditLogs_Feature",
                table: "AiAuditLogs",
                column: "Feature");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_UserId",
                table: "AiConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AiForecastRuns_ForecastFrom_ForecastTo",
                table: "AiForecastRuns",
                columns: new[] { "ForecastFrom", "ForecastTo" });

            migrationBuilder.CreateIndex(
                name: "IX_AiForecastValues_ForecastDate_MenuItemId_HourOfDay",
                table: "AiForecastValues",
                columns: new[] { "ForecastDate", "MenuItemId", "HourOfDay" });

            migrationBuilder.CreateIndex(
                name: "IX_AiForecastValues_ForecastRunId",
                table: "AiForecastValues",
                column: "ForecastRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AiForecastValues_MenuItemId",
                table: "AiForecastValues",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AiInventoryRecommendationDecisions_RecommendationId",
                table: "AiInventoryRecommendationDecisions",
                column: "RecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiInventoryRecommendations_CreatedAt",
                table: "AiInventoryRecommendations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiInventoryRecommendations_RawItemId_Status",
                table: "AiInventoryRecommendations",
                columns: new[] { "RawItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AiMessageRecords_ConversationId",
                table: "AiMessageRecords",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiToolExecutionRecords_ConversationId",
                table: "AiToolExecutionRecords",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAuditLogs");

            migrationBuilder.DropTable(
                name: "AiForecastValues");

            migrationBuilder.DropTable(
                name: "AiInventoryRecommendationDecisions");

            migrationBuilder.DropTable(
                name: "AiMessageRecords");

            migrationBuilder.DropTable(
                name: "AiToolExecutionRecords");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "AiForecastRuns");

            migrationBuilder.DropTable(
                name: "AiInventoryRecommendations");

            migrationBuilder.DropTable(
                name: "AiConversations");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "MinimumOrderQuantity",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "SafetyStockQuantity",
                table: "RawItems");

            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                table: "RawItems");
        }
    }
}
