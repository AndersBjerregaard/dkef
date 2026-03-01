using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dkef_api.Migrations.ForgotPassword
{
    /// <inheritdoc />
    public partial class FixForgotPasswordForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint from ForgotPasswords to Contact
            migrationBuilder.DropForeignKey(
                name: "FK_ForgotPasswords_Contact_ContactId",
                table: "ForgotPasswords");

            // Drop the incorrectly created Contact table
            migrationBuilder.DropTable(
                name: "Contact");

            // Add foreign key to the existing AspNetUsers table
            migrationBuilder.AddForeignKey(
                name: "FK_ForgotPasswords_AspNetUsers_ContactId",
                table: "ForgotPasswords",
                column: "ContactId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key to AspNetUsers
            migrationBuilder.DropForeignKey(
                name: "FK_ForgotPasswords_AspNetUsers_ContactId",
                table: "ForgotPasswords");

            // Recreate the Contact table (if reverting)
            // Note: This is the original schema but data would be lost
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            // Recreate the foreign key to Contact table
            migrationBuilder.AddForeignKey(
                name: "FK_ForgotPasswords_Contact_ContactId",
                table: "ForgotPasswords",
                column: "ContactId",
                principalTable: "Contact",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
