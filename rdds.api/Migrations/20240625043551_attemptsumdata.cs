using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class attemptsumdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cef21c96-72fb-46d4-b717-4d5d56824c3b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e72f6bea-b66a-40bb-938c-50eeac7b2ab0");

            migrationBuilder.AddColumn<string>(
                name: "SummaryId",
                table: "Attempts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttemptSummaryDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalLength = table.Column<double>(type: "double precision", nullable: false),
                    RoadLength_Baik = table.Column<double>(type: "double precision", nullable: true),
                    RoadLength_Sedang = table.Column<double>(type: "double precision", nullable: true),
                    RoadLength_RusakRingan = table.Column<double>(type: "double precision", nullable: true),
                    RoadLength_RusakBerat = table.Column<double>(type: "double precision", nullable: true),
                    Percent_Baik = table.Column<double>(type: "double precision", nullable: true),
                    Percent_Sedang = table.Column<double>(type: "double precision", nullable: true),
                    Percent_RusakRingan = table.Column<double>(type: "double precision", nullable: true),
                    Percent_RusakBerat = table.Column<double>(type: "double precision", nullable: true),
                    AttemptId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttemptSummaryDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttemptSummaryDatas_Attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "Attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1a9849e0-7ed0-4e3c-807a-3710050298e5", null, "Admin", "ADMIN" },
                    { "d07c4f71-abcd-4acf-aa15-1f97ad685c88", null, "User", "USER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttemptSummaryDatas_AttemptId",
                table: "AttemptSummaryDatas",
                column: "AttemptId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttemptSummaryDatas");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1a9849e0-7ed0-4e3c-807a-3710050298e5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d07c4f71-abcd-4acf-aa15-1f97ad685c88");

            migrationBuilder.DropColumn(
                name: "SummaryId",
                table: "Attempts");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "cef21c96-72fb-46d4-b717-4d5d56824c3b", null, "User", "USER" },
                    { "e72f6bea-b66a-40bb-938c-50eeac7b2ab0", null, "Admin", "ADMIN" }
                });
        }
    }
}
