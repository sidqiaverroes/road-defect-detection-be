using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class ResetDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    { "40a43151-dd52-462e-a8d7-e424d8b22f34", null, "User", "USER" },
                    { "76df5571-2a67-4157-816a-faa3a6829c24", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "40a43151-dd52-462e-a8d7-e424d8b22f34");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "76df5571-2a67-4157-816a-faa3a6829c24");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4a94237c-282e-484b-bb0f-ce681721ae4e", null, "Admin", "ADMIN" },
                    { "6d1f3e33-dc3e-4802-b24b-34261003a28a", null, "User", "USER" }
                });
        }
    }
}
