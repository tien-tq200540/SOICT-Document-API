using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOICT.DocumentSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedByToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadedBy",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "Documents");
        }
    }
}
