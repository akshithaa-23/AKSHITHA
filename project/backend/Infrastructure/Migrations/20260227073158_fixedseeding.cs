using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixedseeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LifeCoverage",
                table: "Policies");

            migrationBuilder.AddColumn<int>(
                name: "LifeCoverageMultiplier",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxLifeCoverageLimit",
                table: "Policies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CoverageStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomineeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomineeRelationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomineePhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuoteRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndustryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfEmployees = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedAgentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteRequests_Users_AssignedAgentId",
                        column: x => x.AssignedAgentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuoteRequests_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 200000m, 2, 3000000m, 3000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 200000m, 3, 3000000m, 4500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 200000m, 200000m, 4, 3000000m, 6000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 300000m, 3, 6000000m, 6000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 300000m, 300000m, 4, 6000000m, 8500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 500000m, 300000m, 5, 6000000m, 11000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 300000m, 500000m, 4, 10000000m, 10000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 500000m, 500000m, 5, 10000000m, 14000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 1000000m, 500000m, 7, 10000000m, 18000m });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyId",
                table: "Employees",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteRequests_AssignedAgentId",
                table: "QuoteRequests",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteRequests_CustomerId",
                table: "QuoteRequests",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "QuoteRequests");

            migrationBuilder.DropColumn(
                name: "LifeCoverageMultiplier",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "MaxLifeCoverageLimit",
                table: "Policies");

            migrationBuilder.AddColumn<decimal>(
                name: "LifeCoverage",
                table: "Policies",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { 300000m, null, 5000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { 300000m, 1000000m, 7500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { 500000m, 300000m, 1000000m, 9000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { 500000m, null, 9000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { null, 500000m, 2500000m, 12500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { 1000000m, 500000m, 2500000m, 15000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { null, 1000000m, null, 15000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { null, 1000000m, 5000000m, 20000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverage", "PremiumPerEmployee" },
                values: new object[] { 2000000m, 1000000m, 5000000m, 25000m });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$UxeBAyYr1kgT8JTkWFMRe.JZh.viMDkrtFnQT8OZCMleopeNv1AjW");
        }
    }
}
