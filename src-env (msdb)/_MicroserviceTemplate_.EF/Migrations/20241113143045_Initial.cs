using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _MicroserviceTemplate_.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MyTables",
                columns: table => new
                {
                    MyTableId = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UpdateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyTables", x => x.MyTableId);
                });

            migrationBuilder.InsertData(
                table: "MyTables",
                columns: new[] { "MyTableId", "CreateTimeUtc", "Description", "UpdateTimeUtc", "Value" },
                values: new object[,]
                {
                    { "Operation:Add", new DateTime(2024, 11, 13, 14, 30, 44, 687, DateTimeKind.Utc).AddTicks(2282), "Add a new record.", new DateTime(2024, 11, 13, 14, 30, 44, 687, DateTimeKind.Utc).AddTicks(2282), "Add" },
                    { "Operation:Edit", new DateTime(2024, 11, 13, 14, 30, 44, 687, DateTimeKind.Utc).AddTicks(2286), "Edit existing record.", new DateTime(2024, 11, 13, 14, 30, 44, 687, DateTimeKind.Utc).AddTicks(2286), "Edit" },
                    { "Operation:Remove", new DateTime(2024, 11, 13, 14, 30, 44, 687, DateTimeKind.Utc).AddTicks(2287), "Remove existing record.", new DateTime(2024, 11, 13, 14, 30, 44, 687, DateTimeKind.Utc).AddTicks(2287), "Remove" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyTables");
        }
    }
}
