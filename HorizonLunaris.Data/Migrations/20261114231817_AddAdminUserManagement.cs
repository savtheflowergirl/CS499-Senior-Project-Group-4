using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorizonLunaris.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "Role" },
                values: new object[] { 4, "admin@example.com", "Demo Administrator", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
