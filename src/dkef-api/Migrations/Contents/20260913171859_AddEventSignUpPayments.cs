using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dkef.Migrations.Contents
{
    /// <inheritdoc />
    public partial class AddEventSignUpPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSignUpPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<string>(type: "text", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: false),
                    AmountMinor = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSignUpPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSignUpPayments_Contents_EventId",
                        column: x => x.EventId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSignUpPayments_EventId_ContactId_Status",
                table: "EventSignUpPayments",
                columns: new[] { "EventId", "ContactId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EventSignUpPayments_PaymentId",
                table: "EventSignUpPayments",
                column: "PaymentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSignUpPayments");
        }
    }
}
