using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorizonLunaris.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManualReviewGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresReview",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RequiresReview",
                table: "Tasks");
        }
    }
}
