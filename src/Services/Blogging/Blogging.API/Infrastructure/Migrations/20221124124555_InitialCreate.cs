using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blog.Services.Blogging.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "outbox");

            migrationBuilder.EnsureSchema(
                name: "blogging");

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

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "blogging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "varchar(50)", nullable: false),
                    type = table.Column<string>(type: "varchar(50)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", nullable: false),
                    authorname = table.Column<string>(name: "author_name", type: "varchar(32)", nullable: false),
                    rowversion = table.Column<byte[]>(name: "row_version", type: "bytea", rowVersion: true, nullable: true),
                    authorid = table.Column<Guid>(name: "author_id", type: "uuid", nullable: true),
                    createdat = table.Column<Instant>(name: "created_at", type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    editorname = table.Column<string>(name: "editor_name", type: "varchar(32)", nullable: true),
                    editorid = table.Column<Guid>(name: "editor_id", type: "uuid", nullable: true),
                    editedat = table.Column<Instant>(name: "edited_at", type: "timestamp with time zone", nullable: true),
                    viewscount = table.Column<int>(name: "views_count", type: "integer", nullable: false, defaultValue: 0),
                    likescount = table.Column<int>(name: "likes_count", type: "integer", nullable: false, defaultValue: 0),
                    commentscount = table.Column<int>(name: "comments_count", type: "integer", nullable: false, defaultValue: 0),
                    headerimgurl = table.Column<string>(name: "header_img_url", type: "varchar(500)", nullable: false),
                    recipemeal = table.Column<string>(name: "recipe_meal", type: "varchar(50)", nullable: true),
                    recipedifficulty = table.Column<string>(name: "recipe_difficulty", type: "varchar(50)", nullable: true),
                    recipepreparationminutes = table.Column<int>(name: "recipe_preparation_minutes", type: "integer", nullable: true),
                    recipecookingminutes = table.Column<int>(name: "recipe_cooking_minutes", type: "integer", nullable: true),
                    recipeservingscount = table.Column<int>(name: "recipe_servings_count", type: "integer", nullable: true),
                    recipefoodcomposition = table.Column<string>(name: "recipe_food_composition", type: "varchar(50)", nullable: true),
                    recipesongurl = table.Column<string>(name: "recipe_song_url", type: "varchar(500)", nullable: true),
                    recipepreparationmethods = table.Column<List<string>>(name: "recipe_preparation_methods", type: "text[]", nullable: true),
                    recipetastes = table.Column<List<string>>(name: "recipe_tastes", type: "text[]", nullable: true),
                    reviewrating = table.Column<int>(name: "review_rating", type: "integer", nullable: true),
                    reviewitemname = table.Column<string>(name: "review_item_name", type: "varchar(60)", nullable: true),
                    reviewitemwebsiteurl = table.Column<string>(name: "review_item_website_url", type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "requests",
                schema: "blogging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "varchar(100)", nullable: false),
                    createdat = table.Column<Instant>(name: "created_at", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "blogging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "varchar(50)", nullable: false),
                    value = table.Column<string>(type: "varchar(50)", nullable: false),
                    rowversion = table.Column<byte[]>(name: "row_version", type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_translations",
                schema: "blogging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    postid = table.Column<Guid>(name: "post_id", type: "uuid", nullable: false),
                    language = table.Column<string>(type: "varchar(50)", nullable: false),
                    title = table.Column<string>(type: "varchar(60)", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "varchar(200)", nullable: false),
                    rowversion = table.Column<byte[]>(name: "row_version", type: "bytea", rowVersion: true, nullable: true),
                    translationtype = table.Column<string>(name: "translation_type", type: "varchar(50)", nullable: false),
                    recipedishname = table.Column<string>(name: "recipe_dish_name", type: "varchar(50)", nullable: true),
                    recipecuisine = table.Column<string>(name: "recipe_cuisine", type: "varchar(50)", nullable: true),
                    recipeingredients = table.Column<List<string>>(name: "recipe_ingredients", type: "text[]", nullable: true),
                    reviewrestaurantcountry = table.Column<string>(name: "review_restaurant_country", type: "varchar(50)", nullable: true),
                    reviewrestaurantzipcode = table.Column<string>(name: "review_restaurant_zipcode", type: "varchar(10)", nullable: true),
                    reviewrestaurantcity = table.Column<string>(name: "review_restaurant_city", type: "varchar(50)", nullable: true),
                    reviewrestaurantstreet = table.Column<string>(name: "review_restaurant_street", type: "varchar(50)", nullable: true),
                    reviewrestaurantcuisines = table.Column<List<string>>(name: "review_restaurant_cuisines", type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_translations_posts_post_id",
                        column: x => x.postid,
                        principalSchema: "blogging",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_translations_tags",
                schema: "blogging",
                columns: table => new
                {
                    posttranslationid = table.Column<Guid>(name: "post_translation_id", type: "uuid", nullable: false),
                    tagid = table.Column<Guid>(name: "tag_id", type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_translations_tags", x => new { x.posttranslationid, x.tagid });
                    table.ForeignKey(
                        name: "fk_post_translations_tags_post_translations_post_translation_id",
                        column: x => x.posttranslationid,
                        principalSchema: "blogging",
                        principalTable: "post_translations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_post_translations_tags_tags_tag_id",
                        column: x => x.tagid,
                        principalSchema: "blogging",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.CreateIndex(
                name: "ix_post_translations_base_english",
                schema: "blogging",
                table: "post_translations",
                columns: new[] { "title", "description", "content" })
                .Annotation("Npgsql:TsVectorConfig", "english");

            migrationBuilder.CreateIndex(
                name: "ix_post_translations_post_id_language",
                schema: "blogging",
                table: "post_translations",
                columns: new[] { "post_id", "language" },
                unique: true)
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "ix_post_translations_recipe_english",
                schema: "blogging",
                table: "post_translations",
                columns: new[] { "title", "description", "content", "recipe_cuisine", "recipe_dish_name" })
                .Annotation("Npgsql:TsVectorConfig", "english");

            migrationBuilder.CreateIndex(
                name: "ix_post_translations_tags_tag_id",
                schema: "blogging",
                table: "post_translations_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_author_id",
                schema: "blogging",
                table: "posts",
                column: "author_id")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_posts_category",
                schema: "blogging",
                table: "posts",
                column: "category")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_posts_created_at",
                schema: "blogging",
                table: "posts",
                column: "created_at",
                descending: new bool[0])
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "ix_posts_likes_count",
                schema: "blogging",
                table: "posts",
                column: "likes_count")
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "ix_posts_status_created_at",
                schema: "blogging",
                table: "posts",
                columns: new[] { "status", "created_at" },
                descending: new bool[0])
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "ix_posts_views_count",
                schema: "blogging",
                table: "posts",
                column: "views_count")
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "ix_requests_type",
                schema: "blogging",
                table: "requests",
                column: "type")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_tags_language_value",
                schema: "blogging",
                table: "tags",
                columns: new[] { "language", "value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_value",
                schema: "blogging",
                table: "tags",
                column: "value")
                .Annotation("Npgsql:IndexMethod", "btree");
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

            migrationBuilder.DropTable(
                name: "post_translations_tags",
                schema: "blogging");

            migrationBuilder.DropTable(
                name: "requests",
                schema: "blogging");

            migrationBuilder.DropTable(
                name: "post_translations",
                schema: "blogging");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "blogging");

            migrationBuilder.DropTable(
                name: "posts",
                schema: "blogging");
        }
    }
}
