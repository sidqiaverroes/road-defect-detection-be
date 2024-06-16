using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5db060cd-6514-4449-8797-b580e626891e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cccc8768-5cbe-4f39-9f8d-0dd52fdd6f8a");

            migrationBuilder.AddColumn<int>(
                name: "RoadCategoryId",
                table: "Attempts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoadCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalLength = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadCategories", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_RoadCategoryId",
                table: "Attempts",
                column: "RoadCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attempts_RoadCategories_RoadCategoryId",
                table: "Attempts",
                column: "RoadCategoryId",
                principalTable: "RoadCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attempts_RoadCategories_RoadCategoryId",
                table: "Attempts");

            migrationBuilder.DropTable(
                name: "RoadCategories");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_RoadCategoryId",
                table: "Attempts");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "01d0ad78-f2be-48b7-8178-f14819763f54");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7780bf77-8e9e-4597-bdda-dbe09f1f5d96");

            migrationBuilder.DropColumn(
                name: "RoadCategoryId",
                table: "Attempts");

            migrationBuilder.UpdateData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Accesses",
                value: new[] { "read", "write", "update", "delete" });

            migrationBuilder.UpdateData(
                table: "AccessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Accesses",
                value: new[] { "read" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5db060cd-6514-4449-8797-b580e626891e", null, "User", "USER" },
                    { "cccc8768-5cbe-4f39-9f8d-0dd52fdd6f8a", null, "Admin", "ADMIN" }
                });
        }
    }
}
