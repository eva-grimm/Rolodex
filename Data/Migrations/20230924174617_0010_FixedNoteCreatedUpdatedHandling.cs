using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolodex.Data.Migrations
{
    /// <inheritdoc />
    public partial class _0010_FixedNoteCreatedUpdatedHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Notes",
                newName: "Created");

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Updated",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Notes",
                newName: "CreatedDate");
        }
    }
}
