using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dkef_api.Migrations.Events
{
    /// <inheritdoc />
    public partial class EventsRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileVersion",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "Events",
                newName: "ThumbnailUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailUrl",
                table: "Events",
                newName: "Thumbnail");

            migrationBuilder.AddColumn<string>(
                name: "FileVersion",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
