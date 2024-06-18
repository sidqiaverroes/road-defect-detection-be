using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class CalculatedData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1fc7142a-a59e-4527-bb6d-7160da3f33a6");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ebabbafd-7356-4d7f-b248-339139cbadf7");

            migrationBuilder.RenameColumn(
                name: "Roll",
                table: "CalculatedDatas",
                newName: "PSD_Roll");

            migrationBuilder.AddColumn<float>(
                name: "IRI_Roll",
                table: "CalculatedDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

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
                    { "3b2ed34b-322c-43e1-bb5a-7e994f489561", null, "Admin", "ADMIN" },
                    { "f08f5e74-3527-4b2c-b013-a1415b2d26c9", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3b2ed34b-322c-43e1-bb5a-7e994f489561");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f08f5e74-3527-4b2c-b013-a1415b2d26c9");

            migrationBuilder.DropColumn(
                name: "IRI_Roll",
                table: "CalculatedDatas");

            migrationBuilder.RenameColumn(
                name: "PSD_Roll",
                table: "CalculatedDatas",
                newName: "Roll");

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
                    { "1fc7142a-a59e-4527-bb6d-7160da3f33a6", null, "User", "USER" },
                    { "ebabbafd-7356-4d7f-b248-339139cbadf7", null, "Admin", "ADMIN" }
                });
        }
    }
}
