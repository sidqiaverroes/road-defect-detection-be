using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { "79fa9750-b46c-4f99-bc06-5b1e48973c99", null, "Admin", "ADMIN" },
                    { "871a5fa3-31be-4cfe-910c-3eaa73b115f8", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "79fa9750-b46c-4f99-bc06-5b1e48973c99");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "871a5fa3-31be-4cfe-910c-3eaa73b115f8");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "650c744c-2a8b-42cb-95e9-9e022ac78096", null, "User", "USER" },
                    { "7d92a0af-be60-4224-8805-b8cdd0e130fc", null, "Admin", "ADMIN" }
                });
        }
    }
}
