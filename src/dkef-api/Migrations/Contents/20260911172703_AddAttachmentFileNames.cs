using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dkef.Migrations.Contents
{
    /// <inheritdoc />
    public partial class AddAttachmentFileNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "AttachmentFileNames",
                table: "Contents",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentFileNames",
                table: "Contents");
        }
    }
}
