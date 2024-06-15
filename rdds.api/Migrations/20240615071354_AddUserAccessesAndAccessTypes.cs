using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccessesAndAccessTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "46c064e1-24ae-4a6a-ac79-b63f937b26b3");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ca1c14d3-e38d-4638-b42a-01873733bcab");

            migrationBuilder.CreateTable(
                name: "AccessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccesses",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AccessTypeId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccesses", x => new { x.UserId, x.AccessTypeId });
                    table.ForeignKey(
                        name: "FK_UserAccesses_AccessTypes_AccessTypeId",
                        column: x => x.AccessTypeId,
                        principalTable: "AccessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "48c1eb61-f7a4-4d30-8e69-d90159ddafe5", null, "Admin", "ADMIN" },
                    { "b04ed1da-6fc5-48e9-a972-ea9c119d6275", null, "User", "USER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccesses_AccessTypeId",
                table: "UserAccesses",
                column: "AccessTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccesses");

            migrationBuilder.DropTable(
                name: "AccessTypes");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "48c1eb61-f7a4-4d30-8e69-d90159ddafe5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b04ed1da-6fc5-48e9-a972-ea9c119d6275");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "46c064e1-24ae-4a6a-ac79-b63f937b26b3", null, "User", "USER" },
                    { "ca1c14d3-e38d-4638-b42a-01873733bcab", null, "Admin", "ADMIN" }
                });
        }
    }
}
