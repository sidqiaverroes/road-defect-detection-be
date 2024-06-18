using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class Revamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "Yaw",
                table: "RoadDatas");

            migrationBuilder.DropColumn(
                name: "PSD_Euclidean",
                table: "CalculatedDatas");

            migrationBuilder.DropColumn(
                name: "PSD_Pitch",
                table: "CalculatedDatas");

            migrationBuilder.RenameColumn(
                name: "PSD_Roll",
                table: "CalculatedDatas",
                newName: "IRI_Average");

            migrationBuilder.AddColumn<string>(
                name: "Average_Profile",
                table: "CalculatedDatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Euclidean_Profile",
                table: "CalculatedDatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pitch_Profile",
                table: "CalculatedDatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Roll_Profile",
                table: "CalculatedDatas",
                type: "text",
                nullable: true);

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
                    { "626df129-c663-46cc-bc2d-c1fa8bd7fe75", null, "Admin", "ADMIN" },
                    { "b34673ba-bf9d-4f6d-a8c5-1a32556d7ac4", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "626df129-c663-46cc-bc2d-c1fa8bd7fe75");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b34673ba-bf9d-4f6d-a8c5-1a32556d7ac4");

            migrationBuilder.DropColumn(
                name: "Average_Profile",
                table: "CalculatedDatas");

            migrationBuilder.DropColumn(
                name: "Euclidean_Profile",
                table: "CalculatedDatas");

            migrationBuilder.DropColumn(
                name: "Pitch_Profile",
                table: "CalculatedDatas");

            migrationBuilder.DropColumn(
                name: "Roll_Profile",
                table: "CalculatedDatas");

            migrationBuilder.RenameColumn(
                name: "IRI_Average",
                table: "CalculatedDatas",
                newName: "PSD_Roll");

            migrationBuilder.AddColumn<float>(
                name: "Yaw",
                table: "RoadDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PSD_Euclidean",
                table: "CalculatedDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PSD_Pitch",
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
    }
}
