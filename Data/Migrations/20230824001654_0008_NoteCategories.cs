using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolodex.Data.Migrations
{
    /// <inheritdoc />
    public partial class _0008_NoteCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Contacts_ContactId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_ContactId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "NoteId",
                table: "Categories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NoteId",
                table: "Categories",
                column: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Notes_NoteId",
                table: "Categories",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Notes_NoteId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_NoteId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NoteId",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "Notes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ContactId",
                table: "Notes",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Contacts_ContactId",
                table: "Notes",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id");
        }
    }
}
