using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PollStation.Migrations
{
    /// <inheritdoc />
    public partial class AddPolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Polls",
                newName: "QrCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QrCode",
                table: "Polls",
                newName: "ImageUrl");
        }
    }
}
