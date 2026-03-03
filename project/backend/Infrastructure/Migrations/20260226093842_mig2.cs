using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 26, 9, 38, 41, 772, DateTimeKind.Utc).AddTicks(3649), "$2a$11$fagGnrIt01dLHC/yLjCgxOT0Immjb2nxllXDlmYNQfCl20CyhKkTq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 26, 9, 9, 54, 814, DateTimeKind.Utc).AddTicks(6205), "$2a$11$X7Bajyuc./EQuRgF9lXCIemLb7pRTpx9hVMiohm0JLplrBIe3kBEO" });
        }
    }
}
