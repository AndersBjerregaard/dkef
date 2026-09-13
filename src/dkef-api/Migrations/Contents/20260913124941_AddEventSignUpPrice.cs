using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dkef.Migrations.Contents
{
    /// <inheritdoc />
    public partial class AddEventSignUpPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SignUpPriceMinor",
                table: "Contents",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignUpPriceMinor",
                table: "Contents");
        }
    }
}
