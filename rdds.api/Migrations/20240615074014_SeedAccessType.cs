using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class SeedAccessType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "48c1eb61-f7a4-4d30-8e69-d90159ddafe5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b04ed1da-6fc5-48e9-a972-ea9c119d6275");

            migrationBuilder.InsertData(
                table: "AccessTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "read" },
                    { 2, "write" },
                    { 3, "update" },
                    { 4, "delete" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4a94237c-282e-484b-bb0f-ce681721ae4e", null, "Admin", "ADMIN" },
                    { "6d1f3e33-dc3e-4802-b24b-34261003a28a", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4a94237c-282e-484b-bb0f-ce681721ae4e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6d1f3e33-dc3e-4802-b24b-34261003a28a");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "48c1eb61-f7a4-4d30-8e69-d90159ddafe5", null, "Admin", "ADMIN" },
                    { "b04ed1da-6fc5-48e9-a972-ea9c119d6275", null, "User", "USER" }
                });
        }
    }
}
