using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedTracingDotNet.Services.Users.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

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
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
