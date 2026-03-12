using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTermLifeClaimFieldsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedPayout",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfDeath",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysInCompany",
                table: "Claims",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NormalPayout",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SuicideExclusionFlag",
                table: "Claims",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustedPayout",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DateOfDeath",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DaysInCompany",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "NormalPayout",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "SuicideExclusionFlag",
                table: "Claims");
        }
    }
}
