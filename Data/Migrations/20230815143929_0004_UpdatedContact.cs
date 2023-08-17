using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolodex.Data.Migrations
{
    /// <inheritdoc />
    public partial class _0004_UpdatedContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "States",
                table: "Contacts",
                newName: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "Contacts",
                newName: "States");
        }
    }
}
