using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamCollabApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DisplayName",
                table: "AspNetUsers",
                column: "DisplayName",
                unique: true,
                filter: "[DisplayName] != ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DisplayName",
                table: "AspNetUsers");
        }
    }
}
