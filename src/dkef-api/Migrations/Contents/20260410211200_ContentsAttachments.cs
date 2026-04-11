using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dkef_api.Migrations.Contents
{
    /// <inheritdoc />
    public partial class ContentsAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Attachments",
                table: "Contents",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attachments",
                table: "Contents");
        }
    }
}
