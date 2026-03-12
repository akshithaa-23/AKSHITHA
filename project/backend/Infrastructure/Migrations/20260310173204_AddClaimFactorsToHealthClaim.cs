using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimFactorsToHealthClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccidentDate",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AgeFactor",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CauseOfDeath",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalApprovedAmount",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FrequencyFactor",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccidentDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AgeFactor",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "CauseOfDeath",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "FinalApprovedAmount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "FrequencyFactor",
                table: "Claims");
        }
    }
}
