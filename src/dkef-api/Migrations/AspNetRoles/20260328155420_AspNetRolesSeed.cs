using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dkef.Migrations.AspNetRoles
{
    /// <inheritdoc />
    public partial class AspNetRolesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { "5a7892eb-6f59-4753-9ec4-3d173010647f", "Admin", "ADMIN", "ca6e84e2-b7c4-4de6-be47-ee673fa69b69" },
                    { "c6850c95-b8a1-4491-a52f-83b47b87b9cd", "Board Member", "BOARD MEMBER", "7f1a1f5a-3b14-434b-8647-7c5190f3f425" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("AspNetRoles", "Id", "5a7892eb-6f59-4753-9ec4-3d173010647f");
            migrationBuilder.DeleteData("AspNetRoles", "Id", "c6850c95-b8a1-4491-a52f-83b47b87b9cd");
        }
    }
}
