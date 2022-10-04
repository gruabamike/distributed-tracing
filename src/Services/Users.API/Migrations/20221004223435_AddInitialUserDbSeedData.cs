using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedTracingDotNet.Services.Users.API.Migrations
{
    public partial class AddInitialUserDbSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "FirstName", "LastName" },
                values: new object[,]
                {
                    { 1, "Khadeeja", "Glenn" },
                    { 2, "Monty", "Vang" },
                    { 3, "Roseanne", "Hodges" },
                    { 4, "Antoine", "Bellamy" },
                    { 5, "Felicia", "Dowling" },
                    { 6, "Sidrah", "Humphries" },
                    { 7, "Carly", "Haas" },
                    { 8, "Isadora", "Greig" },
                    { 9, "Cadi", "Bull" },
                    { 10, "Jaxon", "Gentry" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
