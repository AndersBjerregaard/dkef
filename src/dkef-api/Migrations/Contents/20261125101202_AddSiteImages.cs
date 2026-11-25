using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dkef.Migrations.Contents
{
    /// <inheritdoc />
    public partial class AddSiteImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteImages",
                columns: table => new
                {
                    Group = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Slot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Alt = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteImages", x => new { x.Group, x.Slot });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteImages");
        }
    }
}
