using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorizonLunaris.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeReviewGateAndAssignees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Tasks",
                type: "INTEGER",
                nullable: true);
        }
    }
}
