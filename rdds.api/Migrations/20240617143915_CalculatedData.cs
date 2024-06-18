using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class CalculatedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "033473b3-7bde-41e9-bc79-7441ef21c550");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "04e42a4c-017d-494a-ac50-a06c1b0e890a");

            migrationBuilder.RenameColumn(
                name: "Coordinate_Longitude",
                table: "RoadDatas",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "Coordinate_Latitude",
                table: "RoadDatas",
                newName: "Latitude");

            migrationBuilder.CreateTable(
                name: "CalculatedDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Roll = table.Column<float>(type: "real", nullable: false),
                    PSD_Pitch = table.Column<float>(type: "real", nullable: false),
                    PSD_Euclidean = table.Column<float>(type: "real", nullable: false),
                    IRI_Pitch = table.Column<float>(type: "real", nullable: false),
                    IRI_Euclidean = table.Column<float>(type: "real", nullable: false),
                    Velocity = table.Column<float>(type: "real", nullable: false),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Longitude = table.Column<float>(type: "real", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AttemptId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculatedDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculatedDatas_Attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "Attempts",
                        principalColumn: "Id");
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
                    { "1fc7142a-a59e-4527-bb6d-7160da3f33a6", null, "User", "USER" },
                    { "ebabbafd-7356-4d7f-b248-339139cbadf7", null, "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculatedDatas_AttemptId",
                table: "CalculatedDatas",
                column: "AttemptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculatedDatas");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1fc7142a-a59e-4527-bb6d-7160da3f33a6");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ebabbafd-7356-4d7f-b248-339139cbadf7");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "RoadDatas",
                newName: "Coordinate_Longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "RoadDatas",
                newName: "Coordinate_Latitude");

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
                    { "033473b3-7bde-41e9-bc79-7441ef21c550", null, "Admin", "ADMIN" },
                    { "04e42a4c-017d-494a-ac50-a06c1b0e890a", null, "User", "USER" }
                });
        }
    }
}
