using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolodex.Data.Migrations
{
    /// <inheritdoc />
    public partial class _0007_OneImageNotesForNow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Notes",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Notes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Notes");
        }
    }
}
