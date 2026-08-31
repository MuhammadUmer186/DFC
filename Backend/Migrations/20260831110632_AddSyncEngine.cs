using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncCheckpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeerNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastValue = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncCheckpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncConflicts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AggregateGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncomingVersion = table.Column<long>(type: "bigint", nullable: false),
                    LocalVersion = table.Column<long>(type: "bigint", nullable: false),
                    OriginNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncomingPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resolved = table.Column<bool>(type: "bit", nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    ResolvedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncConflicts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncDeadLetters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AggregateGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    EnvelopeJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Replayed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncDeadLetters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncInbox",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AggregateGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AggregateVersion = table.Column<long>(type: "bigint", nullable: false),
                    OriginNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ConflictId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncInbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncNonces",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nonce = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncNonces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncOutbox",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AggregateGlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AggregateVersion = table.Column<long>(type: "bigint", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CausationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dispatched = table.Column<bool>(type: "bit", nullable: false),
                    DispatchedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncOutbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncCheckpoints_PeerNodeId_Direction_AggregateType",
                table: "SyncCheckpoints",
                columns: new[] { "PeerNodeId", "Direction", "AggregateType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_EventId",
                table: "SyncConflicts",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_Resolved_CreatedAtUtc",
                table: "SyncConflicts",
                columns: new[] { "Resolved", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncDeadLetters_EventId",
                table: "SyncDeadLetters",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncDeadLetters_Replayed_CreatedAtUtc",
                table: "SyncDeadLetters",
                columns: new[] { "Replayed", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncInbox_AggregateType_AggregateGlobalId",
                table: "SyncInbox",
                columns: new[] { "AggregateType", "AggregateGlobalId" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncInbox_EventId",
                table: "SyncInbox",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncNonces_NodeId_Nonce",
                table: "SyncNonces",
                columns: new[] { "NodeId", "Nonce" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncNonces_SeenAtUtc",
                table: "SyncNonces",
                column: "SeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SyncOutbox_Dispatched_Id",
                table: "SyncOutbox",
                columns: new[] { "Dispatched", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncOutbox_EventId",
                table: "SyncOutbox",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncCheckpoints");

            migrationBuilder.DropTable(
                name: "SyncConflicts");

            migrationBuilder.DropTable(
                name: "SyncDeadLetters");

            migrationBuilder.DropTable(
                name: "SyncInbox");

            migrationBuilder.DropTable(
                name: "SyncNonces");

            migrationBuilder.DropTable(
                name: "SyncOutbox");
        }
    }
}
