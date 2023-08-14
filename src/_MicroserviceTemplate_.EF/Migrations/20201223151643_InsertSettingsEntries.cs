using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace _MicroserviceTemplate_.EF.Migrations
{
    public partial class InsertSettingsEntries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateTimeUtc",
                table: "SettingsEntries",
                nullable: false,
                defaultValueSql: "getutcdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTimeUtc",
                table: "SettingsEntries",
                nullable: false,
                defaultValueSql: "getutcdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.InsertData(
                table: "SettingsEntries",
                columns: new[] { "SettingsEntryId", "Description", "Value" },
                values: new object[] { "General:AuthorizationBaseUrl", "Base URL of Authorization service", null });

            migrationBuilder.InsertData(
                table: "SettingsEntries",
                columns: new[] { "SettingsEntryId", "Description", "Value" },
                values: new object[] { "General:AuthorizationAccessToken", "Access token to Authorization service", null });

            migrationBuilder.InsertData(
                table: "SettingsEntries",
                columns: new[] { "SettingsEntryId", "Description", "Value" },
                values: new object[] { "TokenValidation:Audience", "Comma separated audiences allowed", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SettingsEntries",
                keyColumn: "SettingsEntryId",
                keyValue: "General:AuthorizationAccessToken");

            migrationBuilder.DeleteData(
                table: "SettingsEntries",
                keyColumn: "SettingsEntryId",
                keyValue: "General:AuthorizationBaseUrl");

            migrationBuilder.DeleteData(
                table: "SettingsEntries",
                keyColumn: "SettingsEntryId",
                keyValue: "TokenValidation:Audience");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateTimeUtc",
                table: "SettingsEntries",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "getutcdate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTimeUtc",
                table: "SettingsEntries",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "getutcdate()");
        }
    }
}