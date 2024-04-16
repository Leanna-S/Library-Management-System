using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookRequestsToArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_Archives_ArchiveId",
                table: "BookRequests");

            migrationBuilder.DropIndex(
                name: "IX_BookRequests_ArchiveId",
                table: "BookRequests");

            migrationBuilder.DropColumn(
                name: "ArchiveId",
                table: "BookRequests");
        }
    }
}
