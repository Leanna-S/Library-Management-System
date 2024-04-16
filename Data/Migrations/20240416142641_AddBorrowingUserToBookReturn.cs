using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowingUserToBookReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReturns_AspNetUsers_LibrarianId",
                table: "BookReturns");

            migrationBuilder.AddColumn<string>(
                name: "BorrowingUserId",
                table: "BookReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BookReturns_BorrowingUserId",
                table: "BookReturns",
                column: "BorrowingUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReturns_AspNetUsers_BorrowingUserId",
                table: "BookReturns",
                column: "BorrowingUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReturns_AspNetUsers_LibrarianId",
                table: "BookReturns",
                column: "LibrarianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReturns_AspNetUsers_BorrowingUserId",
                table: "BookReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReturns_AspNetUsers_LibrarianId",
                table: "BookReturns");

            migrationBuilder.DropIndex(
                name: "IX_BookReturns_BorrowingUserId",
                table: "BookReturns");

            migrationBuilder.DropColumn(
                name: "BorrowingUserId",
                table: "BookReturns");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReturns_AspNetUsers_LibrarianId",
                table: "BookReturns",
                column: "LibrarianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
