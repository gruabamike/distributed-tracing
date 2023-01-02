using System;
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Khadeeja", "Glenn" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Monty", "Vang" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Roseanne", "Hodges" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Antoine", "Bellamy" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Felicia", "Dowling" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Sidrah", "Humphries" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Carly", "Haas" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Isadora", "Greig" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "Cadi", "Bull" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Jaxon", "Gentry" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
