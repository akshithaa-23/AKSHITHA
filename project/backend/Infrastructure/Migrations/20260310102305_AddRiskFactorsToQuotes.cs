using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskFactorsToQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Employees");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseQuote",
                table: "Quotes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GeographyFactor",
                table: "Quotes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IndustryFactor",
                table: "Quotes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanRiskFactor",
                table: "Quotes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CustomIndustry",
                table: "QuoteRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GeographyFactor",
                table: "QuoteRequests",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IndustryFactor",
                table: "QuoteRequests",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCategory",
                table: "QuoteRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanRiskFactor",
                table: "QuoteRequests",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseQuote",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "GeographyFactor",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "IndustryFactor",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "PlanRiskFactor",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "CustomIndustry",
                table: "QuoteRequests");

            migrationBuilder.DropColumn(
                name: "GeographyFactor",
                table: "QuoteRequests");

            migrationBuilder.DropColumn(
                name: "IndustryFactor",
                table: "QuoteRequests");

            migrationBuilder.DropColumn(
                name: "LocationCategory",
                table: "QuoteRequests");

            migrationBuilder.DropColumn(
                name: "PlanRiskFactor",
                table: "QuoteRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
