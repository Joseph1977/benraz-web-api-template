using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace _MicroserviceTemplate_.EF.Migrations
{
    public partial class AddSettingsEntries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettingsEntries",
                columns: table => new
                {
                    SettingsEntryId = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    CreateTimeUtc = table.Column<DateTime>(nullable: false),
                    UpdateTimeUtc = table.Column<DateTime>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsEntries", x => x.SettingsEntryId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettingsEntries");
        }
    }
}