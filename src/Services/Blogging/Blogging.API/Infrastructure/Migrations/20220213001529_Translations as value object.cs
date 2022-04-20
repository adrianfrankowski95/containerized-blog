using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blog.Services.Blogging.API.Migrations
{
    public partial class Translationsasvalueobject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "blogging");

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "blogging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "varchar(50)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", nullable: false),
                    author_name = table.Column<string>(type: "varchar(50)", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    editor_name = table.Column<string>(type: "varchar(50)", nullable: true),
                    editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    edited_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    views_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    likes_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    comments_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    header_img_url = table.Column<string>(type: "varchar(500)", nullable: true),
                    type = table.Column<string>(type: "varchar(50)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    recipe_meal = table.Column<string>(type: "varchar(50)", nullable: true),
                    recipe_difficulty = table.Column<string>(type: "varchar(50)", nullable: true),
                    recipe_preparation_minutes = table.Column<int>(type: "integer", nullable: true),
                    recipe_cooking_minutes = table.Column<int>(type: "integer", nullable: true),
                    recipe_servings_count = table.Column<int>(type: "integer", nullable: true),
                    recipe_food_composition = table.Column<string>(type: "varchar(50)", nullable: true),
                    recipe_song_url = table.Column<string>(type: "varchar(500)", nullable: true),
                    recipe_preparation_methods = table.Column<List<string>>(type: "text[]", nullable: true),
                    recipe_tastes = table.Column<List<string>>(type: "text[]", nullable: true),
                    review_item_type = table.Column<string>(type: "varchar(50)", nullable: true),
                    review_rating = table.Column<int>(type: "integer", nullable: true),
                    review_item_name = table.Column<string>(type: "varchar(60)", nullable: true),
                    review_item_website_url = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "blogging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "varchar(50)", nullable: false),
                    value = table.Column<string>(type: "varchar(50)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
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
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(60)", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "varchar(200)", nullable: false),
                    translation_type = table.Column<string>(type: "varchar(50)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    recipe_dish_name = table.Column<string>(type: "varchar(50)", nullable: true),
                    recipe_cuisine = table.Column<string>(type: "varchar(50)", nullable: true),
                    recipe_ingredients = table.Column<List<string>>(type: "text[]", nullable: true),
                    review_restaurant_country = table.Column<string>(type: "varchar(50)", nullable: true),
                    review_restaurant_zipcode = table.Column<string>(type: "varchar(10)", nullable: true),
                    review_restaurant_city = table.Column<string>(type: "varchar(50)", nullable: true),
                    review_restaurant_street = table.Column<string>(type: "varchar(50)", nullable: true),
                    review_restaurant_cuisines = table.Column<List<string>>(type: "text[]", nullable: true),
                    language = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_translations_posts_post_id",
                        column: x => x.post_id,
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
                    post_translation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_translations_tags", x => new { x.post_translation_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_post_translations_tags_post_translations_post_translation_id",
                        column: x => x.post_translation_id,
                        principalSchema: "blogging",
                        principalTable: "post_translations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_post_translations_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "blogging",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

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
                column: "created_at")
                .Annotation("Npgsql:IndexMethod", "btree")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });

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
                columns: new[] { "status", "created_at" })
                .Annotation("Npgsql:IndexMethod", "btree")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });

            migrationBuilder.CreateIndex(
                name: "ix_posts_views_count",
                schema: "blogging",
                table: "posts",
                column: "views_count")
                .Annotation("Npgsql:IndexMethod", "btree");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_translations_tags",
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
