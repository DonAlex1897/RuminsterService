using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuminsterBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserRoleNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_InitiatorId",
                table: "UserRelationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_ReceiverId",
                table: "UserRelationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRelations_AspNetUsers_InitiatorId",
                table: "UserRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRelations_AspNetUsers_ReceiverId",
                table: "UserRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_InitiatorId",
                table: "UserRelationLogs",
                column: "InitiatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_ReceiverId",
                table: "UserRelationLogs",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelations_AspNetUsers_InitiatorId",
                table: "UserRelations",
                column: "InitiatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelations_AspNetUsers_ReceiverId",
                table: "UserRelations",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_InitiatorId",
                table: "UserRelationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_ReceiverId",
                table: "UserRelationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRelations_AspNetUsers_InitiatorId",
                table: "UserRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRelations_AspNetUsers_ReceiverId",
                table: "UserRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_InitiatorId",
                table: "UserRelationLogs",
                column: "InitiatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationLogs_AspNetUsers_ReceiverId",
                table: "UserRelationLogs",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelations_AspNetUsers_InitiatorId",
                table: "UserRelations",
                column: "InitiatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelations_AspNetUsers_ReceiverId",
                table: "UserRelations",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
