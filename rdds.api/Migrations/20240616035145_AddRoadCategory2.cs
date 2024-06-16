using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadCategory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "01d0ad78-f2be-48b7-8178-f14819763f54");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7780bf77-8e9e-4597-bdda-dbe09f1f5d96");

            migrationBuilder.AlterColumn<float>(
                name: "TotalLength",
                table: "RoadCategories",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.UpdateData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Accesses",
                value: new List<string> { "read", "write", "update", "delete" });

            migrationBuilder.UpdateData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Accesses",
                value: new List<string> { "read" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3b621712-e520-4c86-be66-fea9f05693fb", null, "Admin", "ADMIN" },
                    { "d0c86b3f-52a5-4789-88cf-5c372eb7fdac", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3b621712-e520-4c86-be66-fea9f05693fb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d0c86b3f-52a5-4789-88cf-5c372eb7fdac");

            migrationBuilder.AlterColumn<double>(
                name: "TotalLength",
                table: "RoadCategories",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.UpdateData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Accesses",
                value: new List<string> { "read", "write", "update", "delete" });

            migrationBuilder.UpdateData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Accesses",
                value: new List<string> { "read" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "01d0ad78-f2be-48b7-8178-f14819763f54", null, "User", "USER" },
                    { "7780bf77-8e9e-4597-bdda-dbe09f1f5d96", null, "Admin", "ADMIN" }
                });
        }
    }
}
