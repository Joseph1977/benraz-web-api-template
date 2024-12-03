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
                    MyTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    UpdateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
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
                    { new Guid("8ec418ae-69cf-44f0-a1f3-ba26142b3563"), new DateTime(2024, 12, 2, 13, 36, 9, 545, DateTimeKind.Utc).AddTicks(8224), "Remove existing record.", new DateTime(2024, 12, 2, 13, 36, 9, 545, DateTimeKind.Utc).AddTicks(8224), "Remove" },
                    { new Guid("c209d3b5-6482-439a-be59-3471c170cae9"), new DateTime(2024, 12, 2, 13, 36, 9, 545, DateTimeKind.Utc).AddTicks(8217), "Add a new record.", new DateTime(2024, 12, 2, 13, 36, 9, 545, DateTimeKind.Utc).AddTicks(8217), "Add" },
                    { new Guid("e5ecdbee-834f-42e7-a25d-b0aa00d3fcf1"), new DateTime(2024, 12, 2, 13, 36, 9, 545, DateTimeKind.Utc).AddTicks(8223), "Edit existing record.", new DateTime(2024, 12, 2, 13, 36, 9, 545, DateTimeKind.Utc).AddTicks(8223), "Edit" }
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
