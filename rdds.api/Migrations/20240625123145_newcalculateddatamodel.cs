using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace rdds.api.Migrations
{
    /// <inheritdoc />
    public partial class newcalculateddatamodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1a9849e0-7ed0-4e3c-807a-3710050298e5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d07c4f71-abcd-4acf-aa15-1f97ad685c88");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "CalculatedDatas",
                newName: "LongitudeStart");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "CalculatedDatas",
                newName: "LatitudeStart");

            migrationBuilder.AddColumn<float>(
                name: "LatitudeEnd",
                table: "CalculatedDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "LongitudeEnd",
                table: "CalculatedDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "16b06892-a86f-42e0-9f4c-01196081f502", null, "Admin", "ADMIN" },
                    { "706eaf20-b21d-4bcd-ae07-be98600ff3e8", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "16b06892-a86f-42e0-9f4c-01196081f502");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "706eaf20-b21d-4bcd-ae07-be98600ff3e8");

            migrationBuilder.DropColumn(
                name: "LatitudeEnd",
                table: "CalculatedDatas");

            migrationBuilder.DropColumn(
                name: "LongitudeEnd",
                table: "CalculatedDatas");

            migrationBuilder.RenameColumn(
                name: "LongitudeStart",
                table: "CalculatedDatas",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "LatitudeStart",
                table: "CalculatedDatas",
                newName: "Latitude");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1a9849e0-7ed0-4e3c-807a-3710050298e5", null, "Admin", "ADMIN" },
                    { "d07c4f71-abcd-4acf-aa15-1f97ad685c88", null, "User", "USER" }
                });
        }
    }
}
