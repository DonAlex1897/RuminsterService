using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RuminsterBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    rumination_id = table.Column<int>(type: "integer", nullable: true),
                    parent_comment_id = table.Column<int>(type: "integer", nullable: true),
                    create_by_id = table.Column<string>(type: "text", nullable: false),
                    update_by_id = table.Column<string>(type: "text", nullable: false),
                    create_tms = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_tms = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_comments_asp_net_users_create_by_id",
                        column: x => x.create_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comments_asp_net_users_update_by_id",
                        column: x => x.update_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comments_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comments_ruminations_rumination_id",
                        column: x => x.rumination_id,
                        principalTable: "ruminations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comments_create_by_id",
                table: "comments",
                column: "create_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_parent_comment_id",
                table: "comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_rumination_id",
                table: "comments",
                column: "rumination_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_update_by_id",
                table: "comments",
                column: "update_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comments");
        }
    }
}
