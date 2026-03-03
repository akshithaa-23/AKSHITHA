using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimsmanagerToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MaxLifeCoverageLimit",
                table: "Policies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "LifeCoverageMultiplier",
                table: "Policies",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "AccidentClaimRaised",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "HealthCoverageRemaining",
                table: "Employees",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClaimsManagerId",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyPolicyId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ClaimsManagerId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AccidentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccidentPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimsManagerNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_CompanyPolicies_CompanyPolicyId",
                        column: x => x.CompanyPolicyId,
                        principalTable: "CompanyPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Claims_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Claims_Users_ClaimsManagerId",
                        column: x => x.ClaimsManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Claims_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LifeCoverageMultiplier", "MaxLifeCoverageLimit" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 300000m, 2, 1000000m, 5000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 400000m, 3, 1500000m, 7000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { null, null, 80, 6500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "IsPopular", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { null, 400000m, true, 3, 4000000m, 80, 9500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "HealthCoverage", "IsPopular", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 500000m, false, 4, 5000000m, 80, 12500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AccidentCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { null, null, null, 200, 12000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { null, 700000m, 4, 200, 16000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 1000000m, 5, 15000000m, 200, 21000m });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ClaimsManagerId",
                table: "Companies",
                column: "ClaimsManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ClaimsManagerId",
                table: "Claims",
                column: "ClaimsManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_CompanyPolicyId",
                table: "Claims",
                column: "CompanyPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_CustomerId",
                table: "Claims",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_EmployeeId",
                table: "Claims",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_ClaimsManagerId",
                table: "Companies",
                column: "ClaimsManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Users_ClaimsManagerId",
                table: "Companies");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ClaimsManagerId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AccidentClaimRaised",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HealthCoverageRemaining",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ClaimsManagerId",
                table: "Companies");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxLifeCoverageLimit",
                table: "Policies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LifeCoverageMultiplier",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LifeCoverageMultiplier", "MaxLifeCoverageLimit" },
                values: new object[] { 2, 3000000m });

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
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "PremiumPerEmployee" },
                values: new object[] { 200000m, 4, 3000000m, 6000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 3, 6000000m, 25, 6000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "IsPopular", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 300000m, 300000m, false, 4, 6000000m, 25, 8500m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "HealthCoverage", "IsPopular", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 300000m, true, 5, 6000000m, 25, 11000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AccidentCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 300000m, 4, 10000000m, 50, 10000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AccidentCoverage", "HealthCoverage", "LifeCoverageMultiplier", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 500000m, 500000m, 5, 50, 14000m });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "HealthCoverage", "LifeCoverageMultiplier", "MaxLifeCoverageLimit", "MinEmployees", "PremiumPerEmployee" },
                values: new object[] { 500000m, 7, 10000000m, 50, 18000m });
        }
    }
}
