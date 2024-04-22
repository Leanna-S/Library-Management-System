using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveToBookRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_AspNetUsers_RequesterId",
                table: "BookRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_Books_BookId",
                table: "BookRequests");

            migrationBuilder.AddColumn<int>(
                name: "ArchiveId",
                table: "BookRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookRequests_ArchiveId",
                table: "BookRequests",
                column: "ArchiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRequests_Archives_ArchiveId",
                table: "BookRequests",
                column: "ArchiveId",
                principalTable: "Archives",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BookRequests_AspNetUsers_RequesterId",
                table: "BookRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRequests_Books_BookId",
                table: "BookRequests",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_Archives_ArchiveId",
                table: "BookRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_AspNetUsers_RequesterId",
                table: "BookRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_Books_BookId",
                table: "BookRequests");

            migrationBuilder.DropIndex(
                name: "IX_BookRequests_ArchiveId",
                table: "BookRequests");

            migrationBuilder.DropColumn(
                name: "ArchiveId",
                table: "BookRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRequests_AspNetUsers_RequesterId",
                table: "BookRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookRequests_Books_BookId",
                table: "BookRequests",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
