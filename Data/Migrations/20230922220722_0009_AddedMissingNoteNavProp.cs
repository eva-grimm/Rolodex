using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolodex.Data.Migrations
{
    /// <inheritdoc />
    public partial class _0009_AddedMissingNoteNavProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "CategoryNote",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    NotesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryNote", x => new { x.CategoriesId, x.NotesId });
                    table.ForeignKey(
                        name: "FK_CategoryNote_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryNote_Notes_NotesId",
                        column: x => x.NotesId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryNote_NotesId",
                table: "CategoryNote",
                column: "NotesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryNote");

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
    }
}
