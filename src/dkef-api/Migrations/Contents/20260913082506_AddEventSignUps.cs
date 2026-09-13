using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dkef.Migrations.Contents
{
    /// <inheritdoc />
    public partial class AddEventSignUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SignUpDeadline",
                table: "Contents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventSignUps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<string>(type: "text", nullable: false),
                    SignedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSignUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSignUps_Contents_EventId",
                        column: x => x.EventId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSignUps_ContactId",
                table: "EventSignUps",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSignUps_EventId_ContactId",
                table: "EventSignUps",
                columns: new[] { "EventId", "ContactId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSignUps");

            migrationBuilder.DropColumn(
                name: "SignUpDeadline",
                table: "Contents");
        }
    }
}
