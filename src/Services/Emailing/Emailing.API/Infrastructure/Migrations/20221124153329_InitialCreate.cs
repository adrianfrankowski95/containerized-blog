using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blog.Services.Emailing.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "outbox");

            migrationBuilder.CreateTable(
                name: "inbox_state",
                schema: "outbox",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    messageid = table.Column<Guid>(name: "message_id", type: "uuid", nullable: false),
                    consumerid = table.Column<Guid>(name: "consumer_id", type: "uuid", nullable: false),
                    lockid = table.Column<Guid>(name: "lock_id", type: "uuid", nullable: false),
                    rowversion = table.Column<byte[]>(name: "row_version", type: "bytea", rowVersion: true, nullable: true),
                    received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    receivecount = table.Column<int>(name: "receive_count", type: "integer", nullable: false),
                    expirationtime = table.Column<DateTime>(name: "expiration_time", type: "timestamp with time zone", nullable: true),
                    consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lastsequencenumber = table.Column<long>(name: "last_sequence_number", type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_state", x => x.id);
                    table.UniqueConstraint("ak_inbox_state_message_id_consumer_id", x => new { x.messageid, x.consumerid });
                });

            migrationBuilder.CreateTable(
                name: "message",
                schema: "outbox",
                columns: table => new
                {
                    sequencenumber = table.Column<long>(name: "sequence_number", type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enqueuetime = table.Column<DateTime>(name: "enqueue_time", type: "timestamp with time zone", nullable: true),
                    senttime = table.Column<DateTime>(name: "sent_time", type: "timestamp with time zone", nullable: false),
                    headers = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    inboxmessageid = table.Column<Guid>(name: "inbox_message_id", type: "uuid", nullable: true),
                    inboxconsumerid = table.Column<Guid>(name: "inbox_consumer_id", type: "uuid", nullable: true),
                    outboxid = table.Column<Guid>(name: "outbox_id", type: "uuid", nullable: true),
                    messageid = table.Column<Guid>(name: "message_id", type: "uuid", nullable: false),
                    contenttype = table.Column<string>(name: "content_type", type: "character varying(256)", maxLength: 256, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    conversationid = table.Column<Guid>(name: "conversation_id", type: "uuid", nullable: true),
                    correlationid = table.Column<Guid>(name: "correlation_id", type: "uuid", nullable: true),
                    initiatorid = table.Column<Guid>(name: "initiator_id", type: "uuid", nullable: true),
                    requestid = table.Column<Guid>(name: "request_id", type: "uuid", nullable: true),
                    sourceaddress = table.Column<string>(name: "source_address", type: "character varying(256)", maxLength: 256, nullable: true),
                    destinationaddress = table.Column<string>(name: "destination_address", type: "character varying(256)", maxLength: 256, nullable: true),
                    responseaddress = table.Column<string>(name: "response_address", type: "character varying(256)", maxLength: 256, nullable: true),
                    faultaddress = table.Column<string>(name: "fault_address", type: "character varying(256)", maxLength: 256, nullable: true),
                    expirationtime = table.Column<DateTime>(name: "expiration_time", type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message", x => x.sequencenumber);
                });

            migrationBuilder.CreateTable(
                name: "outbox_state",
                schema: "outbox",
                columns: table => new
                {
                    outboxid = table.Column<Guid>(name: "outbox_id", type: "uuid", nullable: false),
                    lockid = table.Column<Guid>(name: "lock_id", type: "uuid", nullable: false),
                    rowversion = table.Column<byte[]>(name: "row_version", type: "bytea", rowVersion: true, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lastsequencenumber = table.Column<long>(name: "last_sequence_number", type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_state", x => x.outboxid);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inbox_state_delivered",
                schema: "outbox",
                table: "inbox_state",
                column: "delivered");

            migrationBuilder.CreateIndex(
                name: "ix_message_enqueue_time",
                schema: "outbox",
                table: "message",
                column: "enqueue_time");

            migrationBuilder.CreateIndex(
                name: "ix_message_expiration_time",
                schema: "outbox",
                table: "message",
                column: "expiration_time");

            migrationBuilder.CreateIndex(
                name: "ix_message_inbox_message_id_inbox_consumer_id_sequence_number",
                schema: "outbox",
                table: "message",
                columns: new[] { "inbox_message_id", "inbox_consumer_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_message_outbox_id_sequence_number",
                schema: "outbox",
                table: "message",
                columns: new[] { "outbox_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_state_created",
                schema: "outbox",
                table: "outbox_state",
                column: "created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_state",
                schema: "outbox");

            migrationBuilder.DropTable(
                name: "message",
                schema: "outbox");

            migrationBuilder.DropTable(
                name: "outbox_state",
                schema: "outbox");
        }
    }
}
