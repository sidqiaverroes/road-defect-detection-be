using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class userpermssionrelationshipp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8f2dba8d-3612-4b25-a0eb-0a10668531df");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ffa4166a-8edc-462d-bbc7-cd7388c2f466");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "650c744c-2a8b-42cb-95e9-9e022ac78096", null, "User", "USER" },
                    { "7d92a0af-be60-4224-8805-b8cdd0e130fc", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "650c744c-2a8b-42cb-95e9-9e022ac78096");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7d92a0af-be60-4224-8805-b8cdd0e130fc");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8f2dba8d-3612-4b25-a0eb-0a10668531df", null, "User", "USER" },
                    { "ffa4166a-8edc-462d-bbc7-cd7388c2f466", null, "Admin", "ADMIN" }
                });
        }
    }
}
